using System;
using System.Collections.Generic;
using SabberStoneBasicAI.PartialObservation;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.Score;
using System.Diagnostics;

namespace SabberStoneBasicAI.AIAgents.Visionpack
{
    class MyMCTS : LarsWagnerBot
	{
		public PlayerTask lastTask = null; //last Task to get to this node; null for root
		public MyMCTS parent = null;
		public LinkedList<MyMCTS> children;
		public double[] scores; //Q(s, a)
		public int depth = 0;
		public int bestScore = Int32.MinValue;
		public int lastTaskPos =-1;
		public int visitCounter = 0; //N(s)
		public int[] actionfromThisStateCounter = null;  //N(s, a)
		public List<PlayerTask> options;
		public POGame simGame;
		public int nrOfUnvisitedNodes = 1;
		public int timeFrame = 20;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="poGame"></param>
		/// <param name="task"></param>
		/// <param name="n"></param>
		/// <param name="unnesPlayCardTasks"></param>
		public MyMCTS(POGame poGame, PlayerTask task, int n)// List<PlayerTask> options, List<PlayerTask> list, 
		{
			this.children = new LinkedList<MyMCTS>();
			this.lastTask = task;
			this.simGame = poGame; 
			this.depth = n+1;
			//Console.WriteLine("Depth: " + this.depth);
		}

		public MyMCTS()
		{
		}

			/// <summary>
			/// 
			/// </summary>
		public MyMCTS Selection(MyMCTS root)
		{
			//Console.WriteLine("Selection\n");
			MyMCTS node = root;
			while (node.children.Count != 0)
			{				
				node = UCB1.findBestNodeWithUCB1(node);
			}			
			return node;
		}

		/// <summary>
		/// 
		/// </summary>
		public MyMCTS Expansion(MyMCTS nodeToExpand)
		{
			List<PlayerTask> ptList = new List<PlayerTask>();
			
			if (nodeToExpand.lastTask != null && nodeToExpand.lastTask.PlayerTaskType == PlayerTaskType.END_TURN)//terminal node?
			{
				//end expansion
				return nodeToExpand;
			}
			else
			{
				if (nodeToExpand.lastTask != null)//not root
				{
					ptList.Add(nodeToExpand.lastTask);
					nodeToExpand.simGame = nodeToExpand.simGame.Simulate(ptList).Values.Last();					
				}
				nodeToExpand.options = nodeToExpand.simGame.CurrentPlayer.Options();

				//reduce options, remove bad tasks
				reduceOptions(nodeToExpand);

				foreach (PlayerTask pt in nodeToExpand.options)//use reduced options
				{
					nrOfUnvisitedNodes++;
					AddChild(nodeToExpand, pt, nodeToExpand.depth);
				}

				//3 Roll out
				//Console.WriteLine("Roll out\n");
				Random rand = new Random();
				int x = rand.Next(0, nodeToExpand.children.Count);
				nodeToExpand.lastTaskPos = x;
				return Expansion(nodeToExpand.children.ElementAt(x));
			}	
		}

		/// <summary>
		/// 
		/// </summary>
		public void BackPropagation(MyMCTS nodeToBackProp)
		{
			MyMCTS node = nodeToBackProp;
			int score = Score(node.simGame, node.simGame.CurrentPlayer.PlayerId); 

			nodeToBackProp.bestScore = score; //give leafnode its score
											  //in non-leafes it gives the best found score in its branches

			//only root should have null parent, so node is root after loop
			while (node != null)
			{
				//update stuff
				if (node.visitCounter == 0)
				{
					nrOfUnvisitedNodes--;
				}
				node.visitCounter++;  //N(s)
				

				if (node.children.Count != 0)//don't update N(s, a) & Q(s, a) for terminal node
				{
					if (node.actionfromThisStateCounter == null)
					{
						node.actionfromThisStateCounter = new int[node.children.Count];
					}
					node.actionfromThisStateCounter[node.lastTaskPos]++;  //N(s, a)

					if (node.scores == null)
					{
						node.scores = new double[node.children.Count];
					}
					int temp = node.actionfromThisStateCounter[node.lastTaskPos];
					node.scores[node.lastTaskPos] = ((temp - 1) / temp) * node.scores[node.lastTaskPos] + (1 / temp) * score; //Q(a,s), average reward for choosing action a this state s
				}

				if (score > node.bestScore)
				{
					node.bestScore = score;
				}
				node = node.parent;
			} 
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="poGame"></param>
		/// <param name="task"></param>
		/// <param name="n"></param>
		/// <param name="unnecessaryPlayCardTasks"></param>
		public void AddChild(MyMCTS nodeToExpand, PlayerTask task, int n)
		{
			MyMCTS childNode = new MyMCTS(nodeToExpand.simGame, task, n) { parent = nodeToExpand };
			nodeToExpand.children.AddLast(childNode);
			if (nodeToExpand == null)
			{
				Console.WriteLine("Some thing went wrong! Parent is null!");
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="poGame"></param>
		public List<PlayerTask> findNextMove(POGame poGame)
		{
			MyMCTS tree = new MyMCTS(poGame, null, -1);
			Stopwatch s = new Stopwatch();
			s.Start();
			while (s.Elapsed < TimeSpan.FromSeconds(timeFrame))//endcondition
			{
				///Console.WriteLine("Elapsed time: " + s.Elapsed + "  Timespan" + TimeSpan.FromSeconds(20) + "\n");
				//1 Tree selection
				MyMCTS promisingNode = Selection(tree);

				//2&3 Expansion & roll out
				MyMCTS terminalNode = Expansion(promisingNode);

				if (nrOfUnvisitedNodes == 0)//stop early if all existing nodes have been visited (tree is complete and has been searched)
				{
					break;
				}

				//4 Back-propagation
				//int playoutResult = Simulate(...);//??? int?
				BackPropagation(terminalNode);///???
			}
			s.Stop();

			MyMCTS node = new MyMCTS();
			node = tree;
			int winningScore = tree.bestScore;
			List<PlayerTask> bestTasksequence = new List<PlayerTask>();
			while (node.children.Count != 0)
			{
				//Console.WriteLine("While");
				foreach (MyMCTS mcst in node.children)
				{
					if(mcst.bestScore == winningScore)
					{
						node = mcst;
						bestTasksequence.Add(node.lastTask);
						//Console.WriteLine("Break");
						break;
					}
				}
			}

			//tree.PrintPretty("", true);

			return bestTasksequence;
		}

		/// <summary>
		/// Method to print tree structure; used to check correctness and completness of tree; only used for tests
		/// coopiert von https://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure
		/// </summary>
		/// <param name="indent"></param>
		/// <param name="last"></param>
		public void PrintPretty(string indent, bool last)
		{
			Console.Write(indent);
			if (last)
			{
				Console.Write("\\-");
				indent += "  ";
			}
			else
			{
				Console.Write("|-");
				indent += "| ";
			}
			Console.WriteLine(this.lastTask+" Visited: "+this.visitCounter);

			for (int i = 0; i < children.Count; i++)
				children.ElementAt(i).PrintPretty(indent, i == children.Count - 1);
		}


		/// <summary>
		/// return is of Play_Card-tasks that are further to the front of the options list than task
		/// </summary>
		/// <param name="reducePlayCardTasksList"></param>
		/// <param name="options"></param>
		/// <param name="task"></param>
		/// <returns></returns>
		public List<string> reducePlayCardTasks(List<string> reducePlayCardTasksList, List<PlayerTask> options, PlayerTask task)
		{
			foreach (PlayerTask pt in options)//Add all PlayCard-Tasks before task to the list
			{
				string ptSring = pt.ToString();
				if (PlayerTaskType.PLAY_CARD == pt.PlayerTaskType)
				{
					if (ptSring == task.ToString())
					{
						return reducePlayCardTasksList;
					}
					if (!reducePlayCardTasksList.Contains(ptSring))
					{
						reducePlayCardTasksList.Add(ptSring);
					}
				}
			}
			return reducePlayCardTasksList;
		}

		// Calculate different scores based on our hero's class
		//copied from greedyAgent
		private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			switch (state.CurrentPlayer.HeroClass)
			{
				//case CardClass.WARRIOR: return new AggroScore { Controller = p }.Rate();
				case CardClass.MAGE: return new ControlScore { Controller = p }.Rate();
				//case CardClass.ROGUE: return new ControlScore { Controller = p }.Rate();
				case CardClass.SHAMAN: return new ControlScore { Controller = p }.Rate();
				default: return new MidRangeScore { Controller = p }.Rate(); //MidRangeScore
			}
		}

		//check if List contains at least one Minion_attack
		public bool ContainsMinionAttack(List<PlayerTask> list)
		{
			foreach (PlayerTask pt in list)
			{
				if (pt.PlayerTaskType is PlayerTaskType.MINION_ATTACK) return true;
			}
			return false;
		}

		/// <summary>
		/// This method reduces the number of options and with that the number of branches in the search tree by
		/// eliminating bad or unnecessary options and options that have most likely no impact on the result (e.g. different
		/// Positions for cards).
		/// This is supposed to reduce the size of the searchtree and therefore increase the speed at wich a solution is found
		/// </summary>
		/// <param name="node"></param>
		public void reduceOptions(MyMCTS node)
		{
			List<PlayerTask> toBeRemoved = new List<PlayerTask>();

			int i = 0;
			while (i < node.options.Count)
			{
				//remove bad use of MAGE Hero_Power from Options
				if (node.simGame.CurrentPlayer.HeroClass == CardClass.MAGE && node.options[i].PlayerTaskType == PlayerTaskType.HERO_POWER
					&& node.options[i].Target.Controller.PlayerId == node.simGame.CurrentPlayer.PlayerId)
				{
					toBeRemoved.Add(node.options[i]);
				}

				//some Hero_Powers are better used first or make no difference if used first or later or later but before attacks
				//so reduce number of Branches in tree by not making these Hero_Powers an option after frist turn
				if(node.options[i].PlayerTaskType == PlayerTaskType.HERO_POWER && node.depth>1 && new int[] { 10, 8, 2, 3, 4, 5 }.Contains((int)node.simGame.CurrentPlayer.HeroClass))
				{
					toBeRemoved.Add(node.options[i]);
				}

				if (i > 0)
				{
					//don't consider all possible play Positions for Play_Card tasks, makes mostly no difference
					if (node.options[i - 1].PlayerTaskType == PlayerTaskType.PLAY_CARD && node.options[i].PlayerTaskType == PlayerTaskType.PLAY_CARD)
					{
						string ptString = node.options[i-1].ToString();
						int pos = ptString.IndexOf('(');

						if (pos != -1)
						{
							string ptStringNext = node.options[i].ToString();
							int posNext = ptStringNext.IndexOf('(');
							if (pos == posNext && ptString.Remove(pos) == ptStringNext.Remove(pos))
							{
								toBeRemoved.Add(node.options[i]);
							}
						}
					}
				}
				
				i++;
			}

			foreach (PlayerTask t in toBeRemoved)
			{
				node.options.Remove(t);
			}
		}
		
		//check if task is Minion attack; return also false if it is null
		public bool taskIsMinionAttack(PlayerTask task)
		{
			if (task != null)
			{
				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK)
				{
					return true;
				}
			}
			return false;
		}
	}

}
