using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneBasicAI.Score
{
	public class My_Score_m : Score
	{
		public override int Rate()
		{
			double playerScore = 0.0;
			double oponnetScore = 0.0;
			if (OpHeroHp < 1)
				return int.MaxValue;

			if (HeroHp < 1)
				return int.MinValue;

			int result = 0;

			if (Controller.BaseMana - Controller.OverloadLocked - Controller.UsedMana > 0)
				result += (Controller.BaseMana - Controller.OverloadLocked - Controller.UsedMana) * -10;

			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
				result += 1000;

			result += (BoardZone.Count - OpBoardZone.Count) * 75;

			if (Controller.HeroClass.Equals(SabberStoneCore.Enums.CardClass.WARRIOR))
				result += Controller.HeroPowerActivationsThisTurn * 10;

			if (Controller.HeroClass.Equals(SabberStoneCore.Enums.CardClass.HUNTER)
				|| Controller.HeroClass.Equals(SabberStoneCore.Enums.CardClass.MAGE))
				result += Controller.HeroPowerActivationsThisTurn * 20;

			/*if (OpMinionTotHealthTaunt > 0)
				result += MinionTotHealthTaunt * -50;*/




			/*
			if (OpHandCnt < 3)
			{
				oponnetScore += (OpHandCnt + 10) * 2;
			}
			else if (OpHandCnt > 3)
			{
				oponnetScore += 5 + (OpHandCnt + 10) * 2;
			}*/



			
			result += (MinionTotHealth - OpMinionTotAtk) * 100;

			result += OpMinionTotHealthTaunt * -50;

			
			if (OpHeroHp > 0 && OpHeroHp < 5)
				result += OpHeroHp * -25;

			
			result += Controller.NumFriendlyMinionsThatDiedThisGame * -50;

			
			result += Controller.NumFriendlyMinionsThatAttackedThisTurn * 20;

			result += MinionTotAtk;

			result += (HeroHp - OpHeroHp) * 10;

			return result;
		}

	}
}
