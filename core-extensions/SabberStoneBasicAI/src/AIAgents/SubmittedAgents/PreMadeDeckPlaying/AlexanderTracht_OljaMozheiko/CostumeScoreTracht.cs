using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.AIAgents.Costume
{
	public class CostumeScoreTracht : Score.Score
	{
		//double[] weights_p = new double[]  { 1.0,	0.5,	1.0,	1.0,	1.0,	1.0,	1.0 };
		//double[] weights_op = new double[] { 1.25,	1.0,	1.0,	1.0,	1.0,	1.0,	1.0 };

		// actually ok
		//double[] weights_p = new double[]  { 1.0,	1.0,	1.0,	1.0,	1.0,	1.0,	1.0 };
		//double[] weights_op = new double[] { 1.0,	1.0,	1.0,	1.0,	1.0,	1.0,	1.0 };

		//good-ish
		double[] weights_p = new double[]  { 0.75, 0.25, 0.25, 0.75, 0.75, 1.0, 1.0 };
		double[] weights_op = new double[] { 1.25, 0.75, 0.75, 1.0, 0.75, 0.5, 1.5 };


		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			double rate = 0;


			rate += weights_p[0] * HeroHp / 40.0;
			rate -= weights_op[0] * OpHeroHp / 40.0;

			rate += weights_p[1] * HeroAtk / 5.0;
			rate -= weights_op[1] * OpHeroAtk / 5.0;

			rate += weights_p[2] * HandCnt / 10.0;
			rate -= weights_op[2] * OpHandCnt / 10.0;

			rate += weights_p[3] * BoardZone.Count / 7.0;
			rate -= weights_op[3] * OpBoardZone.Count / 7.0;

			rate += weights_p[4] * MinionTotAtk / 20.0;
			rate -= weights_op[4] * OpMinionTotAtk / 20.0;

			rate += weights_p[5] * 0.25 * MinionTotHealth / 20.0;
			rate -= weights_op[5] * 0.25 * OpMinionTotHealth / 20.0;

			rate += weights_p[6] * MinionTotHealthTaunt / 10.0;
			rate -= weights_op[6] * OpMinionTotHealthTaunt / 10.0;


			return (int) (rate*1000);
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
			//return Mull;
		}

		private static List<int> Mull(List<IPlayable> cards)
		{
			return new List<int>();
		}

	}
}
