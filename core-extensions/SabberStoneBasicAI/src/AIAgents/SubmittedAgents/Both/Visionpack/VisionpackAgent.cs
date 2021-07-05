using System;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using System.Diagnostics;
using System.Collections.Generic;

namespace SabberStoneBasicAI.AIAgents.Visionpack
{
	class VisionpackAgent : AbstractAgent
	{
		private Random Rnd = new Random();
		public List<PlayerTask> taskList = new List<PlayerTask>();
		public int highestScore = Int32.MinValue;
		public List<PlayerTask> bestTaskSequence = null;
		public Stopwatch s = new Stopwatch();
		public bool errorOccured = false;

		public override void FinalizeAgent()
		{

		}

		public override void FinalizeGame()
		{

		}

		public override PlayerTask GetMove(POGame poGame)
		{

			try
			{
				if (taskList.Count == 0)
				{
					if (!errorOccured)
					{
						//Console.WriteLine("new Watch");
						s = new Stopwatch();
						s.Start();
					}

					MyMCTS tree = new MyMCTS();

					//new MCST after error; prevent turn from taking too long
					if (s.Elapsed > TimeSpan.FromSeconds(30 - tree.timeFrame))
					{
						return beGreedy(poGame);
					}
					//Console.WriteLine(poGame.FullPrint());
					taskList = tree.findNextMove(poGame);
				}
				PlayerTask nextMove = taskList.First();
				taskList.RemoveAt(0);

				List<PlayerTask> p = new List<PlayerTask>();
				p.Add(nextMove);
				if (poGame.Simulate(p).Values.Last() == null)
				{ //check for valid execution; if error use greedy insead (for rest of turn)
					Console.WriteLine("ERROR  1");
					errorOccured = true;

					//reset Values
					highestScore = Int32.MinValue;
					bestTaskSequence = null;
					taskList.Clear();

					return beGreedy(poGame);
				}

				if (nextMove.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					errorOccured = false;
					//Console.WriteLine("time in Seconds: " + s.Elapsed.TotalSeconds);
					//Console.WriteLine("End of turn");
				}

				return nextMove;
			}
			catch //use Greedy falls fehler auftritt
			{
				Console.WriteLine("ERROR");
				errorOccured = true;

				//reset Values
				highestScore = Int32.MinValue;
				bestTaskSequence = null;
				taskList.Clear();

				return beGreedy(poGame);
			}
		}

		public PlayerTask beGreedy(POGame poGame)
		{
			var player = poGame.CurrentPlayer;

			// Get all simulation results for simulations that didn't fail
			var validOpts = poGame.Simulate(player.Options()).Where(x => x.Value != null);

			// If all simulations failed, play end turn option (always exists), else best according to score function
			PlayerTask nextMove = validOpts.Any() ?
				validOpts.OrderBy(x => Score2(x.Value, player.PlayerId)).Last().Key :
				player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			if (nextMove.PlayerTaskType == PlayerTaskType.END_TURN)
			{
				//Console.WriteLine("End of turn");
				//Console.WriteLine("time in Seconds: " + s.Elapsed.TotalSeconds);
				errorOccured = false;
			}
			return nextMove;
		}


		// Calculate different scores based on our hero's class
		//copied from greedyAgent		
		private static int Score2(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			switch (state.CurrentPlayer.HeroClass)
			{
				//case CardClass.WARRIOR: return new AggroScore { Controller = p }.Rate();
				case CardClass.MAGE: return new ControlScore { Controller = p }.Rate();
				//case CardClass.ROGUE: return new ControlScore { Controller = p }.Rate();
				case CardClass.SHAMAN: return new ControlScore { Controller = p }.Rate();
				default: return new MidRangeScore { Controller = p }.Rate();
			}
		}


		public override void InitializeAgent()
		{
			Rnd = new Random();
		}

		public override void InitializeGame()
		{
		}
	}

}
