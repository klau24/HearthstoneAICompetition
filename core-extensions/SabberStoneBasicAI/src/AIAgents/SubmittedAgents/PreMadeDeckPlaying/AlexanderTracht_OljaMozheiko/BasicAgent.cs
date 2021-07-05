using System;
using System.Linq;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Enums;


//Agent for Master-Submission Alexander Tracht, Olja Mozheiko

// choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it
namespace SabberStoneBasicAI.AIAgents.Costume
{
	class BasicAgent : AbstractAgent
	{

		public override void InitializeAgent()
		{
		}

		public override void FinalizeAgent()
		{
		}

		public override void InitializeGame()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(POGame poGame)
		{

			var player = poGame.CurrentPlayer;

			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mull = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mull);
			}

			var sims = poGame.Simulate(poGame.CurrentPlayer.Options());
			var scores = new List<(int, PlayerTask)>(sims.Count);

			foreach (KeyValuePair<PlayerTask, POGame> pair in sims)
			{
				if (pair.Value != null) 
				{
					scores.Add((Score(pair.Value, player.PlayerId), pair.Key));
				}
			}

			scores.Sort((x,y) => x.Item1.CompareTo(y.Item1));


			//Console.WriteLine("Options: "+player.Options().Count);
			//foreach(var sco in scores)
			//{
			//	Console.WriteLine("  " + sco.Item1 + ":" + sco.Item2.PlayerTaskType);
			//}

			var ret = scores.Last().Item2;

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
