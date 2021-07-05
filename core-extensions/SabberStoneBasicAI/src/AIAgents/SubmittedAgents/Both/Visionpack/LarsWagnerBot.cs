using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.Score;


namespace SabberStoneBasicAI.AIAgents.Visionpack
{
	//AI for premade deck track
	class LarsWagnerBot : AbstractAgent
	{
		private int countFlamestrike;
		List<PlayHistoryEntry> oppPlayedCards = new List<PlayHistoryEntry>();

		int _rewardOppNoBoard = 0;
		int _rewardOppNoHand = 0;
		int _rewardBoardDiff = 0;
		int _rewardTauntDiff = 0;
		int _rewardHeroDiff = 0;
		int _rewardHandDiff = 0;
		int _rewardDeckDiff = 0;
		int _rewardMinAttDiff = 0;
		int _rewardMinHealthDiff = 0;
		int _rewardHeroAttDiff = 0;
		int _rewardHandCost = 0;

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
			countFlamestrike = 0;
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			List<int> values = new List<int>();
			List<PlayerTask> turnList = new List<PlayerTask>();

			PlayerTask bestOption = null;
			int bestValue = int.MinValue;

			EAScore jade = new EAScore();
			EAScore warrior = new EAScore();
			EAScore mage = new EAScore();
			EAScore shaman = new EAScore();
			MidRangeScore defaultPolicy = new MidRangeScore();
			

			Controller control = poGame.CurrentPlayer;

			Dictionary<PlayerTask, POGame> simulated = poGame.Simulate(control.Options());


			if (control.Options().Count == 1)
				return control.Options()[0];

			oppPlayedCards = poGame.CurrentOpponent.PlayHistory;

			foreach (PlayHistoryEntry h in oppPlayedCards)
			{
				if (h.SourceCard.Name == "Flamestrike")
				{
					countFlamestrike++;
				}

			}

			foreach (PlayerTask k in simulated.Keys)
			{

				if (k.PlayerTaskType == PlayerTaskType.END_TURN)
					continue;


				if (poGame.CurrentPlayer.BoardZone.Count >= 4 && k.PlayerTaskType == PlayerTaskType.PLAY_CARD && k.Source.Card.Type == CardType.MINION)
					continue;

				if (poGame.CurrentOpponent.HeroClass == CardClass.MAGE && poGame.CurrentOpponent.BaseMana >= 7 &&
					countFlamestrike <= 1 && k.PlayerTaskType == PlayerTaskType.PLAY_CARD && k.Source.Card.Type == CardType.MINION && k.Source.Card[GameTag.HEALTH] <= 4 && poGame.CurrentPlayer.BoardZone.Count >= 2)
				{
					continue;
				}

				//controller of simulated option
				control = simulated.First(x => x.Key == k).Value?.CurrentPlayer;

				if (control == null)
					continue;

				//set controller on rating function
				//controlScore.Controller = control;
				warrior.Controller = control;
				mage.Controller = control;
				shaman.Controller = control;
				defaultPolicy.Controller = control;

				int currentValue = 0;

				//rate current option
				switch (poGame.CurrentPlayer.HeroClass)
				{
					case CardClass.WARRIOR: currentValue=warrior.BetterRate( 853, -706, 979, -493, -214, -678, -491, 283, 81, 471, 660);
											break;
					case CardClass.MAGE:	currentValue=mage.BetterRate( -69, 553, 603, 765, 437, 530, -541, -7, 813, -556, 864);
											break;
					case CardClass.SHAMAN:	currentValue=shaman.BetterRate( -845, -495, -548, 568, 167, -473, -447, 918, 339, 290, 94);
											break;
					default:				currentValue=defaultPolicy.Rate();
											break;
				}

				if (bestValue < currentValue)
				{
					bestValue = currentValue;
					bestOption = k;
				}
			}

			//debug(turnList, values, bestOption, bestValue, poGame);

			return bestOption ??
				   (bestOption = poGame.CurrentPlayer.Options().Find(x => x.PlayerTaskType == PlayerTaskType.END_TURN));
		}

		public override void InitializeAgent()
		{

		}

		public override void InitializeGame()
		{
			countFlamestrike = 0;
		}
	}
}
