using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using System.Collections.Generic;
using SabberStoneBasicAI.Meta;
using System;
using System.Diagnostics;
using SabberStoneBasicAI.AIAgents.TomPeters;


// TODO choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it

// Bachelor Assignment
// Tom Peters
/*
 DeckList:
 public static List<Card> MechHunter => new List<Card>()
		{
			Cards.FromName("Faithful Lumi"),
			Cards.FromName("Faithful Lumi"),
			Cards.FromName("Mecharoo"),
			Cards.FromName("Mecharoo"),
			Cards.FromName("Annoy-o-Tron"),
			Cards.FromName("Annoy-o-Tron"),
			Cards.FromName("Galvanizer"),
			Cards.FromName("Galvanizer"),
			Cards.FromName("Mechwarper"),
			Cards.FromName("Mechwarper"),
			Cards.FromName("Upgradeable Framebot"),
			Cards.FromName("Upgradeable Framebot"),
			Cards.FromName("Venomizer"),
			Cards.FromName("Venomizer"),
			Cards.FromName("Metaltooth Leaper"),
			Cards.FromName("Metaltooth Leaper"),
			Cards.FromName("Spider Bomb"),
			Cards.FromName("Spider Bomb"),
			Cards.FromName("Spider Tank"),
			Cards.FromName("Explodinator"),
			Cards.FromName("Explodinator"),
			Cards.FromName("Jeeves"),
			Cards.FromName("Jeeves"),
			Cards.FromName("Piloted Shredder"),
			Cards.FromName("Piloted Shredder"),
			Cards.FromName("Replicating Menace"),
			Cards.FromName("Replicating Menace"),
			Cards.FromName("Wargear"),
			Cards.FromName("Wargear"),
			Cards.FromName("Zilliax"),
			};
*/
namespace SabberStoneBasicAI.AIAgents.TomPeters
{
	
	class MechaSMOrcAgent: AbstractAgent
	{

		
		public MechaSMOrcAgent()
		{
			preferedDeck = MechSMOrcDeck.MechHunter;
			preferedHero = CardClass.HUNTER;
		}
		public override void InitializeAgent() {
		
		}
		public override void InitializeGame()
		{
			
		}
		public override void FinalizeGame() {
			
		}
		public override void FinalizeAgent() { }


		public override PlayerTask GetMove(POGame game)
		{
			var player = game.CurrentPlayer;
			
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

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

			

			return new MechSMOrcScore { Controller = p }.Rate();

		}
	}
}
