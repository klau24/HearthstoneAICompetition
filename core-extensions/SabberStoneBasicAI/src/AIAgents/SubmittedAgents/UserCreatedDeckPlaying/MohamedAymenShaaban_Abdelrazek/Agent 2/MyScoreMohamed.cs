using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.Score
{
	public class MyScoreMohamed : Score
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

			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
				result += 150;

			result += (BoardZone.Count - OpBoardZone.Count) * 10;

			if (OpMinionTotHealthTaunt > 0)
				result += MinionTotHealthTaunt * -50;

			/*
			if (OpHandCnt < 3)
			{
				oponnetScore += (OpHandCnt + 10) * 2;
			}
			else if (OpHandCnt > 3)
			{
				oponnetScore += 5 + (OpHandCnt + 10) * 2;
			}*/



			result += MinionTotAtk;

			result += (HeroHp - OpHeroHp) * 30;

			result += (MinionTotHealth - OpMinionTotHealth) * 10;

			result += (MinionTotAtk - OpMinionTotAtk) * 20;

			//result += (int)(playerScore - oponnetScore);

			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
