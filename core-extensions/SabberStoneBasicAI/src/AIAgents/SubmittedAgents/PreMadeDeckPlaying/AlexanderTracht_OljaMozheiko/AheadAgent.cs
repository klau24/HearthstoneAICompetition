using System;
using System.Linq;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Enums;


// choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it
namespace SabberStoneBasicAI.AIAgents.Costume
{
	class AheadAgent : AbstractAgent
	{

		private DateTime start;
		private int max_seconds = 28;

		private int best_k = 5;
		private int best_l = 3;

		//private List<List<double>> seconds_taken;
		private List<double> working_seconds;

		public override void InitializeAgent()
		{
			//seconds_taken = new List<List<double>>(100);
		}

		public override void FinalizeAgent()
		{
			//TODO never called?!
			//double total = 0.0;
			//int moves = 0;
			//int games = seconds_taken.Count;

			//foreach (var list in seconds_taken)
			//{
			//	total += list.Sum();
			//	moves += list.Count;
			//}

			//Console.WriteLine("Stats after " + games + " games:");
			//Console.WriteLine("|- Total time spent: " + total.ToString("0.0000") + " s");
			//Console.WriteLine("|- Moves made: " + moves);
			//Console.WriteLine("|- Average time spent: " + (total/moves).ToString("0.0000") + " s/move");
			//Console.WriteLine("|- Average moves: " + (moves/(double) games).ToString("0.0000") + " moves/game");
			//Console.WriteLine("-----------------");
		}

		public override void InitializeGame()
		{
			working_seconds = new List<double>(50);
		}

		public override void FinalizeGame()
		{
			//double total = working_seconds.Sum();
			//int moves = working_seconds.Count;

			//Console.WriteLine("Game Stats:");
			//Console.WriteLine("|- Total time spent: " + total.ToString("0.0000") + " s");
			//Console.WriteLine("|- Moves made: " + moves);
			//Console.WriteLine("|- Average time spent: " + (total/moves).ToString("0.0000") + " s/move");
			//Console.WriteLine("----------");

			//seconds_taken.Add(working_seconds);
		}

		public override PlayerTask GetMove(POGame poGame)
		{

			start = DateTime.UtcNow;

			SabberStoneCore.Model.Entities.Controller player = poGame.CurrentPlayer;

			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mull = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mull);
			}

			List<PlayerTask> opts = poGame.CurrentPlayer.Options();

			//Handle if choose option instead of normal getmove
			//if (!opts.FindAll(x => x.PlayerTaskType == PlayerTaskType.END_TURN).Any())
			//{
			//	var sims = poGame.Simulate(poGame.CurrentPlayer.Options());
			//	var sim_scores = new List<(PlayerTask, int)>(sims.Count);

			//	foreach (KeyValuePair<PlayerTask, POGame> pair in sims)
			//	{
			//		if (pair.Value != null)
			//		{
			//			sim_scores.Add((pair.Key, Score(pair.Value, player.PlayerId)));
			//		}
			//	}

			//	sim_scores.Sort((x, y) => x.Item2.CompareTo(y.Item2));
			//	return sim_scores.Last().Item1;
			//}

			Dictionary<PlayerTask, POGame> sims = poGame.Simulate(poGame.CurrentPlayer.Options());
			
			//based on i of options[i]
			(PlayerTask, int)[] base_opts = new (PlayerTask, int)[sims.Where(x => x.Value != null).Count()];
			//holds the game states/options being simulated
			var current_games = new List<(PlayerTask, POGame, int)>[base_opts.Length];
			//holds the game states/options being simulated
			var next_games = new List<(PlayerTask, POGame, int)>[base_opts.Length];

			//holds the games through options being evaluated in the current i 
			//and then being added to next_games
			var working_games = new List<(PlayerTask, POGame, int)>();
			var working_sims = new Dictionary<PlayerTask, POGame>();


			//TODO nicer way
			int i_1 = 0;

			foreach (var sim in sims)
			{
				if (sim.Value != null)
				{
					int score = Score(sim.Value, player.PlayerId);
					base_opts[i_1] = (sim.Key, score);
					current_games[i_1] = new List<(PlayerTask, POGame, int)> { (sim.Key, sim.Value, score) };
					i_1++;
				}
			}

			bool done = false;

			//TODO
			// - work through every i of base_opts
			// - for each work through all options of current_games
			// - evaluate for each i the list:
			// - task, new game states and score into working_games
			// - update/add best_k to next_games + update base_opts
			// - check time after each i -> premature return

			var passed = new TimeSpan();

			while (!done)
			{

				//TODO test if necessary
				passed = DateTime.UtcNow - start;
				if (passed.TotalSeconds > max_seconds)
				{
					//Console.WriteLine("WARNING (beginning !done): nearly exceeded max_time (" +passed.TotalSeconds.ToString("0.0000")+ "s) passed");
					return base_opts.OrderByDescending(x => x.Item2).First().Item1;
				}

				// set done true, if new game is added to next_games: set to false
				done = true;

				for (int i=0; i<base_opts.Length; i++)
				{
					next_games[i] = new List<(PlayerTask, POGame, int)>(best_k);
					foreach (var state in current_games[i])
					{
						passed = DateTime.UtcNow - start;
						//TODO test if necessary
						if (passed.TotalSeconds > max_seconds)
						{
							//Console.WriteLine("WARNING (foreach state): nearly exceeded max_time (" +passed.TotalSeconds.ToString("0.0000")+ "s) passed");
							return base_opts.OrderByDescending(x => x.Item2).First().Item1;
						}

						if (state.Item1.PlayerTaskType == PlayerTaskType.END_TURN)
						{
							next_games[i].Add(state);
							continue;
						}
						working_sims = state.Item2.Simulate(state.Item2.CurrentPlayer.Options());
						working_games = new List<(PlayerTask, POGame, int)>(working_sims.Count);
						//find best_l and add to next_games and choose afterwards

						int score = 0;

						foreach (var pair in working_sims)
						{
							if (pair.Value != null)
							{
								score = Score(pair.Value, player.PlayerId);

								// update score of base_opts
								if (score > base_opts[i].Item2)
								{
									base_opts[i].Item2 = score;
								}

								working_games.Add((pair.Key, pair.Value, score));
							
							}
						}

						//TODO faster by adding own loops?

						next_games[i].AddRange(working_games.OrderByDescending(x => x.Item3).Take(best_l));
					
					}

					//TODO sort to best_k games

					current_games[i] = next_games[i].OrderByDescending(x => x.Item3).Take(best_k).ToList();

					//TODO stop if all END

					if (current_games[i].Where(x => x.Item1.PlayerTaskType != PlayerTaskType.END_TURN).Any())
					{
						done = false;
					}

				}

			}

			passed = DateTime.UtcNow - start;
			working_seconds.Add(passed.TotalSeconds);
			//Console.WriteLine("Got to the end in " + passed.TotalSeconds + "s");

			PlayerTask ret = base_opts.OrderByDescending(x => x.Item2).First().Item1;


			//Console.WriteLine("Options: "+player.Options().Count);
			//foreach(var opt in base_opts)
			//{
			//	Console.WriteLine("  " + opts.Item2 + ":" + opt.Item1.PlayerTaskType);
			//}

			//Console.WriteLine("Chosen: " + ret.PlayerTaskType);
			//Console.WriteLine("-----------");

			return ret;
		}

		// TODO better scoring
		private static int Score(POGame state, int p_id)
		{
			var p = state.CurrentPlayer.PlayerId == p_id ? state.CurrentPlayer : state.CurrentOpponent;
			return new CostumeScoreTracht { Controller = p }.Rate();
		}


	}
}
