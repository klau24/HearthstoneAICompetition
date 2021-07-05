using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;

namespace SabberStoneAICompetition.src.AIAgents
{
	//0,36 bot4
	class DefaultScore5 : MyScoreThoms
	{
		private int MyMinionScoreReward = 1000;
		private int OppMinionScoreReward = 1000;
		//private int MinionCountDiffReward = 1000;
		private int MinionsKillCountReward = 1000;
		private int DrawCountReward = 1000;
		private int DiscartedCountReward = 5000;
		//private int RemainingManaReward = 1000;
		private int JadeScaleReward = 800;
		private int HealthReward = 200;
		private int OppHealthReward = 300;
		//private int HandCountReward = 1000;
		//private int OppHandCountReward = 1000;
		//private int HandCostReward = 1000;
		//private int OpNoHandReward = 1000;
		//private int DeckCountReward = 1000;
		//private int DeckCountDiffReward = 1000;
		private int MyBoardCountReward = 400;
		private int OpBoardCountReward = 200;
		//private int MinionHealthDiffReward = 1000;
		//private int MinionAttDiffReward = 1000;
		//private int OpNoBoardReward = 5000;
		//private int HeroAttackDifReward = 1000;

		//45 with bot5
		public override int MyRate()
		{
			
			Controller player = Controller;
			Controller opponent = player.Opponent;

			if (OpHp < 1)
				return Int32.MaxValue;
			if (MyHp < 1)
				return Int32.MinValue;


			int result = 0;
			
			
			result += MyHp*HealthReward;
			result -= OpHp * OppHealthReward;

			result += player.NumCardsDrawnThisTurn * DrawCountReward;
			result -= player.NumDiscardedThisGame * DiscartedCountReward;

			//result += MyBoardZone.Count * MyBoardCountReward ;
			//result += OpBoardZone.Count * OpBoardCountReward;

			Minion[] myMinions = player.BoardZone.GetAll();
			Minion[] oppMinions = OpBoardZone.GetAll();

			result += (myMinions.Count() - oppMinions.Count()) * MyMinionScoreReward;

			foreach (Minion minion in myMinions)
			{
				result += getMinionScore(minion) * MyMinionScoreReward;
			}
			foreach (Minion minion in oppMinions)
			{
				result -= getMinionScore(minion) * OppMinionScoreReward;
			}
			


			result += player.NumMinionsPlayerKilledThisTurn * MinionsKillCountReward;

			if(player.HeroClass==CardClass.SHAMAN || player.HeroClass == CardClass.ROGUE || player.HeroClass == CardClass.DRUID)
				result += player.JadeGolem * JadeScaleReward;

			return result;
		}

		

		private int getMinionScore(Minion minion)
		{
			int result = 0;

			result = minion.Health + minion.AttackDamage;
			int baseValue = result;

			if (minion.IsFrozen)
			{
				return minion.Health;
			}

			if (minion.HasTaunt)
			{
				result += 2;
			}

			if (minion.HasWindfury)
			{
				result += (int)(minion.AttackDamage * 0.5);
			}

			if (minion.HasDivineShield)
			{
				result += (int)(baseValue * 1.5);
			}

			if (minion.IsSilenced &&
				(minion.HasTaunt || minion.CantBeTargetedByHeroPowers || minion.HasDeathrattle || minion.HasWindfury
				|| minion.HasLifeSteal || minion.HasInspire || minion.HasOverkill || minion.SpellPower !=0 ))
			{
				result += (int)(baseValue / 0.9);
			}

			if (minion.Poisonous)
			{
				result += (int)(baseValue * 2);
			}

			if (minion.SpellPower > 0)
			{
				result += (int)(baseValue * 1.5);
			}

			if (minion.IsEnraged)
			{
				result += 1;
			}

			if (minion.HasStealth)
			{
				result += 1;
			}

			if (minion.CantBeTargetedBySpells)
			{
				result += (int)(baseValue *1.5);
			}

			return result;
		}



		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
