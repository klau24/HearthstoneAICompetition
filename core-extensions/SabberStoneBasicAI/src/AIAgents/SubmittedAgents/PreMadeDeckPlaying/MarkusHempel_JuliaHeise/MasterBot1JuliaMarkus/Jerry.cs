
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using System.Linq;


// TODO choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it

// Authors: Julia Heise & Markus Hempel
// INF:Master
namespace SabberStoneBasicAI.AIAgents.BotterThanYouThink
{
	class Jerry : AbstractAgent
	{

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
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			var player = poGame.CurrentPlayer;

			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomMidrangeScoreHempel().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				switch(poGame.CurrentPlayer.HeroClass){
					case CardClass.SHAMAN: goto case CardClass.HUNTER;
					case CardClass.PALADIN: goto case CardClass.HUNTER;
					case CardClass.DRUID: goto case CardClass.HUNTER;
					case CardClass.ROGUE: goto case CardClass.HUNTER;
					case CardClass.HUNTER:
						mulligan = new CustomMidrangeScoreHempel().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList()); break;
					case CardClass.WARLOCK: goto case CardClass.WARRIOR;
					case CardClass.WARRIOR:
						mulligan = new CustomAggroScoreHempel().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList()); break;
					case CardClass.PRIEST: goto case CardClass.MAGE;
					case CardClass.MAGE:
						mulligan = new CustomControlScoreHempel().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList()); break;
					default:
						mulligan = new CustomMidrangeScoreHempel().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList()); break;
				}
				return ChooseTask.Mulligan(player, mulligan);
			}


			// Get all simulation results for simulations that didn't fail
			var validOpts = poGame.Simulate(player.Options()).Where(x => x.Value != null);
			
			// List of best pairs 
            var ActionScore = new List<KeyValuePair<PlayerTask, int>>();

            foreach (KeyValuePair<PlayerTask, POGame> pair in validOpts)
            {

                var optionsDeep1 = pair.Value.CurrentPlayer.Options(); //get substate options
				var bestDeepActionState = new KeyValuePair<PlayerTask, POGame>();

				if (optionsDeep1.Any() && pair.Key.PlayerTaskType != PlayerTaskType.END_TURN)
				{
					var validDeep1Options = pair.Value.Simulate(optionsDeep1).Where(x => x.Value != null); //get valid substate options
					if (validDeep1Options.Any())
					{
						bestDeepActionState = validDeep1Options.OrderBy(x => Score(x.Value, player.PlayerId)).Last();
						ActionScore.Add(new KeyValuePair<PlayerTask, int>(pair.Key, (int)(0.2*(double)Score(pair.Value, player.PlayerId) + 0.8 * (double)Score(bestDeepActionState.Value, player.PlayerId))));
					}
					else
					{
						ActionScore.Add(new KeyValuePair<PlayerTask, int>(pair.Key, Score(pair.Value, player.PlayerId)));
					}					
				}
				else
				{
					// collect best tasks per done task
					ActionScore.Add(new KeyValuePair<PlayerTask, int>(pair.Key, Score(pair.Value, player.PlayerId)));
				}				
            }

			// return the action with the best combination of current task and next task
			return validOpts.Any() ? ActionScore.OrderBy(x => x.Value).Last().Key : player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
        }

		// Calculate different scores based on our hero's class
		private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			var score = 0;
			switch (state.CurrentPlayer.HeroClass)
			{
				case CardClass.SHAMAN: goto case CardClass.HUNTER;
				case CardClass.PALADIN: goto case CardClass.HUNTER;
				case CardClass.DRUID: goto case CardClass.HUNTER;
				case CardClass.ROGUE: goto case CardClass.HUNTER;
				case CardClass.HUNTER:
					score = new CustomMidrangeScoreHempel { Controller = p }.Rate(); break;
				case CardClass.WARLOCK: goto case CardClass.WARRIOR;
				case CardClass.WARRIOR:
					score = new CustomAggroScoreHempel { Controller = p }.Rate(); break;
				case CardClass.PRIEST: goto case CardClass.MAGE;
				case CardClass.MAGE:
					score = new CustomControlScoreHempel { Controller = p }.Rate(); break;
				default:
					score = new CustomControlScoreHempel { Controller = p }.Rate(); break;
			}
			return score;
		}
	}
}
