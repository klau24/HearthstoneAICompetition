using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using System.Linq;
using SabberStoneCore.Model.Entities;
using System.Diagnostics;


// TODO choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it


namespace SabberStoneBasicAI.AIAgents.BotterThanYouThink
{
	class PickleRick : AbstractAgent
	{
		Stack<PlayerTask> TaskList;
		bool newTurn;
		double[] weights;
#if DEBUG
		Stopwatch watch;
#endif

		public override void InitializeAgent()
		{

		}
		public override void FinalizeAgent()
		{
		}
		public override void FinalizeGame()
		{
		}
		public override void InitializeGame()
		{
			TaskList = new Stack<PlayerTask>();
			newTurn = true;
			Tree.rnd = new Random();
			weights = new double[3];
#if DEBUG
			watch = new Stopwatch();
#endif
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			Controller player = poGame.CurrentPlayer;

			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new MidRangeScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				switch (poGame.CurrentPlayer.HeroClass)
				{
					case CardClass.SHAMAN: goto case CardClass.HUNTER;
					case CardClass.PALADIN: goto case CardClass.HUNTER;
					case CardClass.DRUID: goto case CardClass.HUNTER;
					case CardClass.HUNTER:
						mulligan = new CustomMidrangeScoreHempel { Controller = player }.MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
						weights = new double[3] { 1.0, 1.0, 1.0 };
						break;
					case CardClass.ROGUE: goto case CardClass.WARRIOR;
					case CardClass.WARLOCK: goto case CardClass.WARRIOR;
					case CardClass.WARRIOR:
						mulligan = new CustomAggroScoreHempel { Controller = player }.MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
						weights = new double[3] { 1.0, 0.3, 0.7 };
						break;
					case CardClass.PRIEST: goto case CardClass.MAGE;
					case CardClass.MAGE:
						mulligan = new CustomScore { Controller = player }.MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
						weights = new double[3] { 0.5, 1.0, 1.0};
						break;
					default:
						mulligan = new CustomMidrangeScoreHempel { Controller = player }.MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
						weights = new double[3] { 1.0, 1.0, 1.0 };
						break;
				}
				return ChooseTask.Mulligan(player, mulligan);
			}

			if (newTurn)
			{
#if DEBUG
				watch.Restart();
#endif
				var tree = new Tree(poGame, weights);
				newTurn = false;
				tree.GetNext();
				TaskList = new Stack<PlayerTask>();
				TaskList = tree.BestEndNode.GetTaskList();
#if DEBUG
				int bestScore = Int32.MinValue;
				bestScore = tree.BestEndNode.score;
				Console.WriteLine(watch.ElapsedMilliseconds + " for a score of " + bestScore + " and a depth of " + TaskList.Count());
#endif
			}
#if DEBUG
			if (watch.ElapsedMilliseconds > 30 * 1000)
			{
				Console.WriteLine("\n TIMES OVER, YOU LOST!");
			}
#endif
			if (TaskList.Any())
			{
#if DEBUG
				
				Console.WriteLine("Do " + TaskList.Peek().ToString());
#endif
				return TaskList.Pop();
			}
			else
			{
#if DEBUG
				Console.WriteLine(watch.ElapsedMilliseconds + "ms for this turn\n");
#endif
				newTurn = true;
				return player.Options().FindLast(x => x.PlayerTaskType == PlayerTaskType.END_TURN) ?? player.Options().First();
			}
		}

		internal class Tree
		{
			internal RicksNode BestEndNode;
			internal Stopwatch watch;
			internal List<RicksNode> nextLevel;
			internal List<RicksNode> currentLevel;
			internal RicksNode root;
			internal double[] weights = new double[3];

			internal static Random rnd;

			internal Tree(POGame poGame, double[] _weights)
			{
				//calculate whole turn only once
				nextLevel = new List<RicksNode>();
				currentLevel = new List<RicksNode>();
				BestEndNode = new RicksNode();
				root = new RicksNode(poGame);
				weights = _weights;
			}

			internal void GetNext()
			{
				StartWatch();
				currentLevel.Add(root);

				while (currentLevel.Any() && CheckTime())
				{
					GetNext(currentLevel.Last());
					currentLevel.RemoveAt(currentLevel.Count() - 1);
					if (!currentLevel.Any())
					{
						currentLevel.AddRange(nextLevel);
						nextLevel.Clear();
					}
				}

				foreach (RicksNode nextnode in nextLevel)
				{
					if(watch.ElapsedMilliseconds <= 25 * 1000)
					{
						nextnode.CalcScore();
						if (nextnode.score >= BestEndNode.score)
						{
							BestEndNode = nextnode;
						}
					}
				}
				foreach (RicksNode nextnode in currentLevel)
				{
					if(watch.ElapsedMilliseconds <= 27 * 1000)
					{
						nextnode.CalcScore();
						if (nextnode.score >= BestEndNode.score)
						{
							BestEndNode = nextnode;
						}
					}
				}

				StopWatch();
			}

			public void StartWatch()
			{
				watch = new Stopwatch();
				watch.Start();
			}

			public void StopWatch()
			{
				watch.Stop();
			}

			internal void GetNext(RicksNode node)
			{
				IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = node.game.Simulate(node.game.CurrentPlayer.Options()).Where(x => x.Value != null);
				int count = validOpts.Count();
				foreach (KeyValuePair<PlayerTask, POGame> opt in validOpts)
				{
					if (opt.Key.PlayerTaskType == PlayerTaskType.END_TURN)
					{
						var newNode = new RicksNode(opt.Value, opt.Key, node);
						newNode.CalcScore();
						if (newNode.score >= BestEndNode.score)
						{
							BestEndNode = newNode;
						}
					}
					else
					{
						int p = rnd.Next(10);
						if (count < 4 || ((opt.Key.PlayerTaskType == PlayerTaskType.HERO_ATTACK  && p < weights[0]*10 )|| (opt.Key.PlayerTaskType == PlayerTaskType.PLAY_CARD && p < weights[1]*10) || (opt.Key.PlayerTaskType == PlayerTaskType.MINION_ATTACK && p < weights[2]*10)))
						{
							nextLevel.Add(new RicksNode(opt.Value, opt.Key, node));
						}
					}
				}
			}

			bool CheckTime()
			{
				if (watch.ElapsedMilliseconds >  23 * 1000)
				{
#if DEBUG
					Console.WriteLine("Time's almost over! Finish the turn! (" + watch.ElapsedMilliseconds + ")");
#endif
					return false;
				}
				else
				{
					return true;
				}
			}
		}


		internal class RicksNode
		{
			internal RicksNode parent;
			internal int score;
			internal PlayerTask task;
			internal readonly POGame game;

			internal RicksNode()
			{
				score = 0;
			}

			internal RicksNode(POGame _game, PlayerTask _task = null, RicksNode _parent = null)
			{
				game = _game;
				parent = _parent;
				task = _task;
				score = 0;
			}

			internal void CalcScore()
			{
				score = Score(game, game.CurrentPlayer.PlayerId);
			}


			internal Stack<PlayerTask> GetTaskList()
			{
				var result = new Stack<PlayerTask>();
				//result.Push(this.task);
				RicksNode next = parent;
				while (next?.parent != null)
				{
					result.Push(next.task);
					next = next.parent;
				}
				return result;
			}
		}

		// Calculate different scores based on our hero's class
		private static int Score(POGame state, int playerId)
		{
			switch (state.CurrentPlayer.HeroClass)
			{
				case CardClass.SHAMAN: goto case CardClass.HUNTER;
				case CardClass.PALADIN: goto case CardClass.HUNTER;
				case CardClass.DRUID: goto case CardClass.HUNTER;
				case CardClass.ROGUE: goto case CardClass.HUNTER;
				case CardClass.HUNTER:
					return new CustomMidrangeScoreHempel { Controller = state.CurrentPlayer.Controller }.Rate();
				case CardClass.WARLOCK: goto case CardClass.WARRIOR;
				case CardClass.WARRIOR:
					return new CustomAggroScoreHempel { Controller = state.CurrentPlayer.Controller }.Rate();
				case CardClass.PRIEST: goto case CardClass.MAGE;
				case CardClass.MAGE:
					return new CustomControlScoreHempel { Controller = state.CurrentPlayer.Controller }.Rate();
				default:
					return new CustomMidrangeScoreHempel { Controller = state.CurrentPlayer.Controller }.Rate();

			}
		}
	}
}
