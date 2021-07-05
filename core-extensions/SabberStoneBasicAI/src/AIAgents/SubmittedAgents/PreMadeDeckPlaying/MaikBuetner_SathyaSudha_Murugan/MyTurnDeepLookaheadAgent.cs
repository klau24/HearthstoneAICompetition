/* 
 * Students: Sathya Sudha Murugan and Maik Buettner 
 * Master Task
 * Namespace: MaBuSaBu
*/

using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using System.Linq;
using System.Diagnostics;


// TODO choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it
namespace SabberStoneBasicAI.AIAgents.MaBuSaBu
{
	class MyTurnDeepLookaheadAgent : AbstractAgent
	{
		private Stopwatch actWatch;
		private Stopwatch turnWatch;
		private int turn = 0;
		private const int SearchDepth = 4;

		private Dictionary<EnumScores, int> scoreDict;		

		public override void InitializeAgent()
		{
			turnWatch = new Stopwatch();
			actWatch = new Stopwatch();
			turn = -1;

			// Initialize Scoring
			scoreDict = new Dictionary<EnumScores, int>();
			scoreDict.Add(EnumScores.HeroHealth, 15);
			scoreDict.Add(EnumScores.EnemyHeroHealth, -20);
			scoreDict.Add(EnumScores.HeroArmor, 15);
			scoreDict.Add(EnumScores.EnemyHeroArmor, -20);

			scoreDict.Add(EnumScores.MobHealth, 15);
			scoreDict.Add(EnumScores.MobAttack, 20);
			scoreDict.Add(EnumScores.EnemyMobHealth, -45);
			scoreDict.Add(EnumScores.EnemyMobAttack, -60);

			scoreDict.Add(EnumScores.MobCount, 10);
			scoreDict.Add(EnumScores.EnemyMobCount, -20);

			scoreDict.Add(EnumScores.PotentialDanger, -10);
		}

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			if (poGame.Turn != turn)
			{
				turnWatch.Restart();
				turn = poGame.Turn;
			}
			actWatch.Restart();
			int actTime = (int)((30000 - turnWatch.ElapsedMilliseconds) * 0.75);
			var player = poGame.CurrentPlayer.PlayerId;

			var openTasks = new Stack<SimGame>();
			openTasks.Push(new SimGame(poGame, SearchDepth));

			// lookahead all solutions until SearchDepth reached
			int bestScore = int.MinValue;
			SimGame bestSim = null;
			while (openTasks.Any())
			{
				var currentSim = openTasks.Pop();
				var children = currentSim.Next();
				for (int j = 0; j < children.Length; j++)
					if (!children[j].hasChildren)
					{
						int score = children[j].GameValue(player, scoreDict);
						if (score > bestScore)
						{
							bestScore = score;
							bestSim = children[j];
						}
					}
					else
						openTasks.Push(children[j]);
				if (actWatch.ElapsedMilliseconds >= actTime)
					break;
			}

			//Debug.WriteLine($"Turn {poGame.Turn}|{turnWatch.ElapsedMilliseconds}ms took {actWatch.ElapsedMilliseconds}ms to find {bestSim.tasks[0]}");
			return bestSim?.tasks[0] ?? poGame.Simulate(poGame.CurrentPlayer.Options()).Keys.First();
		}

		public override void InitializeGame()
		{
		}
	}
}
