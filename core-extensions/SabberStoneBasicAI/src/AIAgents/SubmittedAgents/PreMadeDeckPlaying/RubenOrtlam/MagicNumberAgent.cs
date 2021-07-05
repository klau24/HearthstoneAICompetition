using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Enums;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model;

namespace SabberStoneBasicAI.AIAgents.magic_number
{
	class MagicNumberAgent : AbstractAgent
	{
		private Stopwatch _watch;
		private MagicNumberScore _score;
		private List<double> _scores;
		private List<double[]> _features;
		private Controller _me;

		public override PlayerTask GetMove(POGame game)
		{
			var player = game.CurrentPlayer;
			_me = player;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			int depth;
			int beamWidth;

			// Check how much time we have left on this turn. The hard limit is 75 seconds so we already stop
			// beam searching when 60 seconds have passed, just to be sure.
			if (_watch.ElapsedMilliseconds < 30 * 1000)
			{ // We still have ample time, proceed with beam search
				depth = 15;
				beamWidth = 12;
			}
			else
			{ // Time is running out, just simulate one timestep now
				depth = 1;
				beamWidth = 1;
				Console.WriteLine("Over 30s in turn already. Pausing beam search for this turn!");
			}

			_watch.Start();
			var move = BeamSearch(game, depth, playerbeamWidth: beamWidth, opponentBeamWidth: 1);
			_watch.Stop();

			if (move.PlayerTaskType == PlayerTaskType.END_TURN)
			{
				_watch.Reset();
			}

			return move;
		}

		private void UpdateScoreWeights() {

		}

		private PlayerTask BeamSearch(POGame game, int depth, int playerbeamWidth, int opponentBeamWidth)
		{
			var me = game.CurrentPlayer;


			var bestSimulations = Simulate(game, playerbeamWidth);
			LabelSimulations(bestSimulations, 0);


			for (var i = 1; i < depth; i++)
			{
				var newBestSimulations = new List<Simulation>();
				foreach (var sim in bestSimulations)
				{
					var beamWidth = sim.Game.CurrentPlayer.PlayerId == me.PlayerId
						? playerbeamWidth
						: opponentBeamWidth;
					var childSims = Simulate(sim.Game, beamWidth);
					LabelSimulations(childSims, i);
					childSims.ForEach(x => x.Parent = sim);
					newBestSimulations.AddRange(childSims);
				}

				bestSimulations = newBestSimulations
					.OrderBy(x => Score(x.Game, me.PlayerId))
					.TakeLast(playerbeamWidth)
					.Reverse()
					.ToList();
			}

			if (bestSimulations.Any()) {
				_score.Controller = bestSimulations.First().Game.CurrentPlayer;
				_features.Add(_score.GetFeatureVector());
				_scores.Add(_score.Rate());
			}

			var nextMove = bestSimulations.Any()
				? bestSimulations.First().GetFirstTask()
				: me.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			return nextMove;
		}


		private List<Simulation> Simulate(POGame game, int numSolutions)
		{
			var simulations = game
				.Simulate(game.CurrentPlayer.Options()).Where(x => x.Value != null)
				.Select(x => new Simulation
				{ Task = x.Key, Game = x.Value, Score = Score(x.Value, game.CurrentPlayer.PlayerId) })
				.OrderBy(x => x.Score)
				.TakeLast(numSolutions)
				.Reverse() // Best task first
				.ToList();

			return simulations;
		}


		// Calculate different scores based on our hero's class
		private int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new MagicNumberScore() {Controller = p}.Rate();
		}

		private void LabelSimulations(List<Simulation> simulations, int currentDepth)
		{
			for (var i = 0; i < simulations.Count; i++)
			{
				simulations[i].Label = currentDepth + "-" + i;
			}
		}


		public override void InitializeAgent()
		{
			_score = new MagicNumberScore();
		}

		public override void InitializeGame()
		{
			_watch = new Stopwatch();
			_scores = new List<double>();
			_features = new List<double[]>();
		}

		public override void FinalizeGame()
		{

		}

		public void FinalizeGame(POGame game)
		{
			bool won;
			if (_me.PlayerId == 1) {
				won = game.getGame().Player1.PlayState == PlayState.WON;
			} else {
				won = game.getGame().Player2.PlayState == PlayState.WON;
			}

			bool tied = game.CurrentPlayer.PlayState == PlayState.TIED;
			bool maxTurnsReached = game.getGame().State == State.RUNNING;

			float discount = 0.98f;
			float alpha = 0.00001f;
			
			Console.WriteLine("Gamestate: " + game.getGame().State);
			Console.WriteLine("I am player " + _me.PlayerId);
			Console.WriteLine("State of Player1:" + game.getGame().Player1.PlayState);
			Console.WriteLine("State of Player2:" + game.getGame().Player2.PlayState);
			Console.WriteLine("Won: " + won);
			Console.WriteLine("Tied: " + tied);
			Console.WriteLine("Max turns reached: " + maxTurnsReached);

			double reward = won ? 10 : -10;
			reward = tied ? 0 : reward;
			reward = maxTurnsReached ? -5 : reward;
			
			Console.WriteLine(reward);

			var weights = new List<double>(MagicNumberScore.Factors);
			
			double total_squared_error = 0;
			Console.WriteLine(_scores.Count);
			for (int i = 1; i < _scores.Count; i++) {
				var d_return = reward *  Math.Pow(discount, (_scores.Count() - i - 1));
				double error = d_return - _scores[i];
				Console.WriteLine("Error in turn " + i + ": " + Math.Pow(error, 2));
				total_squared_error += Math.Pow(error, 2);
				var fl = new List<double>(_features[i]).Zip(new List<double>(_features[i-1]), (a, b) => (a,b));
				weights = weights.Zip(fl, (w, f) => (w + alpha * error * (f.b - f.a))).ToList();
			}
			Console.WriteLine("New Weights:");
			for (int i = 0; i < weights.Count(); i++) {
				Console.Write($"{i}: {weights[i]}|");
			}
			MagicNumberScore.Factors = weights.ToArray();
			Console.WriteLine("Normailized Weights:");
			for (int i = 0; i < MagicNumberScore.Factors.Count(); i++) {
				Console.Write($"{i}: {MagicNumberScore.Factors[i]}|");
			}
			Console.WriteLine("-------------");
			Console.WriteLine("Total squared error this game: " + total_squared_error);
		}

		public override void FinalizeAgent()
		{
		}
	}

	class Simulation
	{
		public PlayerTask Task { get; set; }
		public POGame Game { get; set; }
		public int Score { get; set; }
		public Simulation Parent { get; set; }
		public string Label { get; set; } = "<missing>";

		public PlayerTask GetFirstTask()
		{
			return Parent == null ? Task : Parent.GetFirstTask();
		}

		public string GetQualifiedLabel()
		{
			return Parent == null ? Label : Parent.GetQualifiedLabel() + " ==> " + Label;
		}

		public override string ToString()
		{
			return $"{nameof(Task)}: {Task}, {nameof(Score)}: {Score}";
		}
	}
}
