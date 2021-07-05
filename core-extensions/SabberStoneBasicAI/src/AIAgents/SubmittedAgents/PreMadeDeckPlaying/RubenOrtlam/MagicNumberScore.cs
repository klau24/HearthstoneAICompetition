using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.AIAgents.magic_number
{
	public class MagicNumberScore : Score.Score
	{
		public static double[] _factors { get; set; }
		public static bool initialized = false;
		private const int FACTORS = 50;

		public MagicNumberScore() {
			if (!MagicNumberScore.initialized) {
				_factors = new double[] {
					1, -0.313140444427951, 0.0350294142210343, -0.00423951408136424, -0.63135488989981, -0.179070100037625, 0.235867384538191, 0.0818379506981384, 0.106839458006836, 0.819794792433674, -1, 0.730427065621908, -0.919488430937192, 0.130274842228604, -0.129276081175632, 0, -0.00928856536340188, -0.00184011430780823, 0, 0, -4.01332085596184E-09, -0.63135488989981, 0.0121007571841896, 0.0194737808242957, 0.00163299777614446, 0.00179177418302831, 0.0822731219118019, 0.00967416864376188, 0, -7.81673957866551E-07, 0.00543997597475404, -1.64051410391528E-05, 4.67011767431011E-22, 0, 0.000470791036398351, 0.000219634605286937, -0.0145960201778813, -0.0239944043006524, -0.00138388116667496, -0.00411739038204738, -0.102362991604939, -0.0126536979084023, 0, -0.000287046590306531, -0.00472918757353559, 0, 3.64988833780904E-05, 0, -0.00126619203729034, 0
				};

				MagicNumberScore.initialized = true;
			}
		}

		public static double[] Factors
		{
			get {
				return _factors;
			}

			set {
				if (value.Count() != FACTORS) {
					throw new InvalidDataException();
				}
				var min = Math.Abs(value.Min());
				var max = value.Max();
				// Do not normalize if all values are small
				if (max < 1 || min < 1) {
					_factors = value;
					return;
				}
				if (max == 0) max = 1;
				if (min == 0) min = -1;
				for (var i = 0; i < FACTORS; i++) {
					if (value[i] < 0) {
						_factors[i] = value[i] / min;	
					} else {
						_factors[i] = value[i] / max;	
					}
				}
			}
		}

		public double[] GetFeatureVector()
		{
			var features = new double[FACTORS];

			features[0] = (HeroHp);
			features[1] = (OpHeroHp);
			features[2] = (HeroAtk);
			features[3] = (OpHeroAtk);
			features[4] = (HandTotCost);
			features[5] = (HandCnt);
			features[6] = (OpHandCnt);
			features[7] = (DeckCnt);
			features[8] = (OpDeckCnt);

			features[9] = (MinionTotAtk);
			features[10] = (OpMinionTotAtk);
			features[11] = (MinionTotHealth);
			features[12] = (OpMinionTotHealth);
			features[13] = (MinionTotHealthTaunt);
			features[14] = (OpMinionTotHealthTaunt);

			features[15] = (Hand.Sum(x => x.IsExhausted ? 1 : 0));
			features[16] = (Hand.Sum(x => x.Overload));
			features[17] = (Hand.Sum(x => x.HasDeathrattle? 1 : 0));
			features[18] = (Hand.Sum(x => x.HasLifeSteal ? 1 : 0));
			features[19] = (Hand.Sum(x => x.IsEcho ? 1 : 0));
			features[20] = (Hand.Sum(x => x.HasOverkill ? 1 : 0));
			features[21] = (Hand.Sum(x => x.Cost));

			features[22] = (BoardZone.Sum(x => x.SpellPower));
			features[23] = (BoardZone.Sum(x => x.HasCharge ? 1 : 0));
			features[24] = (BoardZone.Sum(x => x.HasDivineShield ? 1 : 0));
			features[25] = (BoardZone.Sum(x => x.HasWindfury ? 1 : 0));
			features[26] = (BoardZone.Sum(x => x.HasBattleCry ? 1 : 0));
			features[27] = (BoardZone.Sum(x => x.HasDeathrattle ? 1 : 0));
			features[28] = (BoardZone.Sum(x => x.HasInspire ? 1 : 0));
			features[29] = (BoardZone.Sum(x => x.IsEnraged ? 1 : 0));
			features[30] = (BoardZone.Sum(x => x.Freeze ? 1 : 0));
			features[31] = (BoardZone.Sum(x => x.Poisonous ? 1 : 0));
			features[32] = (BoardZone.Sum(x => x.HasLifeSteal ? 1 : 0));
			features[33] = (BoardZone.Sum(x => x.Untouchable ? 1 : 0));
			features[34] = (BoardZone.Sum(x => x.IsRush ? 1 : 0));
			features[35] = (BoardZone.Sum(x => x.AttackableByRush ? 1 : 0));

			features[36] = (OpBoardZone.Sum(x => x.SpellPower));
			features[37] = (OpBoardZone.Sum(x => x.HasCharge ? 1 : 0));
			features[38] = (OpBoardZone.Sum(x => x.HasDivineShield ? 1 : 0));
			features[39] = (OpBoardZone.Sum(x => x.HasWindfury ? 1 : 0));
			features[40] = (OpBoardZone.Sum(x => x.HasBattleCry ? 1 : 0));
			features[41] = (OpBoardZone.Sum(x => x.HasDeathrattle ? 1 : 0));
			features[42] = (OpBoardZone.Sum(x => x.HasInspire ? 1 : 0));
			features[43] = (OpBoardZone.Sum(x => x.IsEnraged ? 1 : 0));
			features[44] = (OpBoardZone.Sum(x => x.Freeze ? 1 : 0));
			features[45] = (OpBoardZone.Sum(x => x.Poisonous ? 1 : 0));
			features[46] = (OpBoardZone.Sum(x => x.HasLifeSteal ? 1 : 0));
			features[47] = (OpBoardZone.Sum(x => x.Untouchable ? 1 : 0));
			features[48] = (OpBoardZone.Sum(x => x.IsRush ? 1 : 0));
			features[49] = (OpBoardZone.Sum(x => x.AttackableByRush ? 1 : 0));

			return features;
		}

		public override int Rate()
		{
			var features = GetFeatureVector();
			double result = 0;
			for (int i = 0; i < FACTORS; i ++) {
				result += Factors[i] * features[i];
			}

			return (int) result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
