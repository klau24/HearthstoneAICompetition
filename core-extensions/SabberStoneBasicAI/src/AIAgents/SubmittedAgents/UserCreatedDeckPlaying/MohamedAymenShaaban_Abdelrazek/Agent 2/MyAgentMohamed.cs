using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.AIAgents;
using System;
using SabberStoneBasicAI.Meta;

namespace SabberStoneBasicAI.AIAgents.M_Priest_2
{
	class MyAgentMohamed : AbstractAgent
	{

		public MyAgentMohamed()
		{
			preferedDeck = Decks.RenoKazakusDragonPriest;
			preferedHero = CardClass.PRIEST;

		}

		public override void InitializeAgent() {
		}
		public override void InitializeGame() { }
		public override void FinalizeGame() { }
		public override void FinalizeAgent() { }


		public override PlayerTask GetMove(POGame game)
		{
			var player = game.CurrentPlayer;

			// Get all simulation results for simulations that didn't fail
			var validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);

			// If all simulations failed, play end turn option (always exists), else best according to score function
			return validOpts.Any() ?
				validOpts.OrderBy(x => Score(x.Value, player.PlayerId)).Last().Key :
				player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
		}

		// Calculate different scores based on our hero's class
		private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			switch (state.CurrentPlayer.HeroClass)
			{
				//dont give cases for now and only return my new score 

				//case CardClass.WARRIOR: return new AggroScore { Controller = p }.Rate();
				//case CardClass.MAGE: return new ControlScore { Controller = p }.Rate();
				default: return new MyScoreMohamed { Controller = p }.Rate();
			}
		}
	
}
}
