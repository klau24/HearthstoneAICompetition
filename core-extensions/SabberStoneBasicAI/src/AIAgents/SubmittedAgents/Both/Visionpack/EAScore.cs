using SabberStoneCore.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SabberStoneBasicAI.AIAgents.Visionpack
{
    class EAScore : LarsScore
    {
		public override int BetterRate(int rewardOppNoBoard, int rewardOppNoHand, int rewardBoardDiff, int rewardTauntDiff, int rewardHeroDiff, int rewardHandDiff, int rewardDeckDiff, int rewardMinAttDiff, int rewardMinHealthDiff, int rewardHeroAttDiff, int rewardHandCost)
		{
			if (OpHeroHp < 1)
				return int.MaxValue;

			if (HeroHp < 1)
				return int.MinValue;

			int result = 0;

			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
				result += rewardOppNoBoard;

			if (OpHandCnt == 0 && HandCnt > 0)
				result += rewardOppNoHand;

			result += (BoardZone.Count - OpBoardZone.Count) * rewardBoardDiff;

			result += (MinionTotHealthTaunt - OpMinionTotHealthTaunt) * rewardTauntDiff;

			result += (HeroHp - OpHeroHp) * rewardHeroDiff;

			result += (HandCnt - OpHandCnt) * rewardHandDiff;

			result += (DeckCnt - OpDeckCnt) * rewardDeckDiff;

			result += (MinionTotAtk - OpMinionTotAtk) * rewardMinAttDiff;

			result += (MinionTotHealth - OpMinionTotHealth) * rewardMinHealthDiff;

			result += (HeroAtk - OpHeroAtk) * rewardHeroAttDiff;

			result += HandTotCost * rewardHandCost;

			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
