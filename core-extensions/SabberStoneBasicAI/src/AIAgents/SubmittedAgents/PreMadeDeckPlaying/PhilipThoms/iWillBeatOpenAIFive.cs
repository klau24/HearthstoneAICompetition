using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneAICompetition.src.AIAgents;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using SabberStoneBasicAI.Score;
using SabberStoneBasicAI.AIAgents;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Zones;
using System.ComponentModel.Design;
using System.Diagnostics.Tracing;
using System.Diagnostics;

namespace SabberStoneBasicAI.AIAgents.FrankenStein
{
	class iWillBeatOpenAIFive : AbstractAgent
	{
		public override void FinalizeAgent()
		{

		}

		public override void FinalizeGame()
		{

		}



		public override void InitializeAgent()
		{
			_treesize = 5;
			_scoreArray=new int[_treesize][];
		}

		public override void InitializeGame()
		{

		}
		private int _treesize;
		private Controller _player;
		private int[][] _scoreArray;
		public override PlayerTask GetMove(POGame poGame)
		{

			_player = poGame.CurrentPlayer;

			if (_player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new DefaultScore().MulliganRule().Invoke(_player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(_player, mulligan);
			}


			Dictionary<KeyValuePair<PlayerTask, POGame>, int> ScoreDict = new Dictionary<KeyValuePair<PlayerTask, POGame>, int>();
			Stopwatch stopwatch = new Stopwatch();
			
			var simulation = poGame.Simulate(_player.Options()).Where(x => x.Value != null);
			simulation = simulation.OrderBy(x => Score(x.Value, _player.PlayerId));
			stopwatch.Start();
		
			foreach(var task in simulation)
			{
				if (stopwatch.ElapsedMilliseconds > 25000)
				{
					break;
				}

				if (task.Key.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					ScoreDict.Add(task, Score(task.Value, _player.PlayerId));
					continue;
				}
				POGame gamecopy = task.Value.getCopy();
				var options = gamecopy.CurrentPlayer.Options();
				var simulationv2 = gamecopy.Simulate(options).Where(x => x.Value != null);
				simulationv2 = simulationv2.OrderBy(x => Score(x.Value, _player.PlayerId));
				Dictionary<KeyValuePair<PlayerTask, POGame>, int> ScoreDict2 = new Dictionary<KeyValuePair<PlayerTask, POGame>, int>();

				foreach (var task2 in simulationv2)
				{
					if (stopwatch.ElapsedMilliseconds > 25000)
					{

						break;
					}

					POGame gamecopy2 = task2.Value.getCopy();
					var options2 = gamecopy2.CurrentPlayer.Options();
					if (task2.Key.PlayerTaskType == PlayerTaskType.END_TURN || options2.Count>20)
					{
						ScoreDict2.Add(task2, Score(task2.Value, _player.PlayerId));
						continue;
					}
					var simulationv3 = gamecopy2.Simulate(options2).Where(x => x.Value != null);
					simulationv3 = simulationv3.OrderBy(x => Score(x.Value, _player.PlayerId));

					//evaluate the best score out of the third simulation and add it in scoredict 2
					ScoreDict2.Add(task2, Score(simulationv3.OrderBy(x => Score(x.Value, _player.PlayerId)).Last().Value, _player.PlayerId));

				}
				ScoreDict.Add(task, ScoreDict2.OrderBy(x => x.Value).Last().Value);
					
			}
			if(stopwatch.ElapsedMilliseconds>25000)
				Console.WriteLine(stopwatch.ElapsedMilliseconds);


			return simulation.Any() ?
			ScoreDict.OrderBy(x => x.Value).Last().Key.Key :
			_player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);


		}

		private Dictionary<PlayerTask,POGame> evalResult(Dictionary<PlayerTask, POGame> simulation, int Treesize)
		{
			Dictionary<PlayerTask, POGame> simulationResult = new Dictionary<PlayerTask, POGame>();
			int result = 0;
			KeyValuePair<PlayerTask, POGame> minPair = new KeyValuePair<PlayerTask, POGame>();
			int minPairScore = 0;


			foreach (KeyValuePair<PlayerTask, POGame> task in simulation)
			{
				if (simulationResult.Count == 0)
					minPair = task;
				if (simulationResult.Count <= Treesize)
				{
					simulationResult.Add(task.Key, task.Value);
				}
				else
				{
					foreach (KeyValuePair<PlayerTask, POGame> listpair in simulationResult)
					{
						minPairScore = Score(minPair.Value, _player.PlayerId);
						int listpairScore = Score(listpair.Value, _player.PlayerId);
						if (listpairScore < minPairScore)
						{
							minPair = listpair;
							minPairScore = listpairScore;
						}
					}
					result = Score(task.Value, _player.PlayerId);
					if (minPairScore < result)
					{
						simulationResult.Add(task.Key, task.Value);
						simulationResult.Remove(minPair.Key);
					}

				}
			}


			return simulationResult;

		}

		private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;

			return new DefaultScore2 { Controller = p }.MyRate();
		}


	}
}
