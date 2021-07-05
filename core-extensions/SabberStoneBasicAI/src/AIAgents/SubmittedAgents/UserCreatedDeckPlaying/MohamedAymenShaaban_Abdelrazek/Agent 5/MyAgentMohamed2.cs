using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.AIAgents;
using System;
using System.Collections.Generic;
using SabberStoneCore.Exceptions;
using SabberStoneCore.Model.Entities;
using SabberStoneBasicAI.Meta;


/// <summary>
/// Deck to be played with this agent is AggroPirateWarrior
/// </summary>

namespace SabberStoneBasicAI.AIAgents.M_Warrior_5
{
	class MyAgentMohamed2 : AbstractAgent
	{

		public MyAgentMohamed2()
		{
			preferedDeck = Decks.AggroPirateWarrior;
			preferedHero = CardClass.WARRIOR;
		}

		public override void InitializeAgent()
		{
		}
		public override void InitializeGame() { }
		public override void FinalizeGame() { }
		public override void FinalizeAgent() { }


		public override PlayerTask GetMove(POGame game)
		{
			var player = game.CurrentPlayer;

			// Get all simulation results for simulations that didn't fail
			var validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);

			//Dictionary<PlayerTask, POGame>

			Dictionary<PlayerTask, POGame> dict1 = game.Simulate(game.CurrentPlayer.Options());

			if (dict1.Any())
			{
				return dict1.OrderBy(s => Score(s.Value, game.CurrentPlayer.PlayerId)).Last().Key;
			}
			else
			{
				return game.CurrentPlayer.Options().First(p => p.PlayerTaskType == PlayerTaskType.END_TURN);
			}
		}

		// Calculate different scores based on our hero's class
		private static int Score(POGame state, int playerId)
		{
			Controller cont = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;

			return new My_Score_m { Controller = cont }.Rate();
		}

	}
}
