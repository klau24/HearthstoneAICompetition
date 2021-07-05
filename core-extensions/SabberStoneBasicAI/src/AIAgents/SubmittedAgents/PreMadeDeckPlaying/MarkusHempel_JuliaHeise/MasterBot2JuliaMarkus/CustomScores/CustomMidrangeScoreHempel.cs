using System;
using System.Collections.Generic;
using SabberStoneCore.Model.Entities;
using System.Linq;
using SabberStoneBasicAI.Score;

namespace SabberStoneBasicAI.AIAgents.BotterThanYouThink
{
	public class CustomMidrangeScoreHempel : Score.Score
	{
		private int Scale { get; set; } = 1000;
		public override int Rate()
		{
			if (HeroHp < 1)
			{
				return Int32.MinValue;
			}

			if (OpHeroHp < 1)
			{
				return Int32.MaxValue;
			}

			int result = 0;

			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
				result += 7 * Scale;

			Controller player = Controller;
			Controller opp = player.Opponent;

			result += player.AmountHeroHealedThisTurn * 5;
			result -= opp.AmountHeroHealedThisTurn * 5;
			result += player.JadeGolem * 10 * Scale;
			result += player.NumCardsDrawnThisTurn * Scale;
			result += player.NumDiscardedThisGame * -5 * Scale;
			result += player.NumFriendlyMinionsThatAttackedThisTurn * Scale;
			result += player.NumMinionsPlayerKilledThisTurn * Scale;
			result += player.RemainingMana * -Scale;

			int heroHp = HeroHp + player.Hero.Armor;
			int oppHeroHp = OpHeroHp + opp.Hero.Armor;
			result += (heroHp - oppHeroHp) * Scale;
			result += HandCnt * 2 * Scale;
			result -= OpHandCnt * 2 * Scale;

			Minion[] minions = BoardZone.GetAll();
			Minion[] oppMinions = OpBoardZone.GetAll();

			result += (minions.Count() - oppMinions.Count()) * 4 * Scale;
			result += getTotalMinionScore(minions);
			result -= getTotalMinionScore(oppMinions);

			if (OpMinionTotHealthTaunt > 0)
				result += OpMinionTotHealthTaunt * -Scale;

			return result;
		}

		private int getTotalMinionScore(Minion[] minions)
		{
			int result = 0;
			foreach (Minion minion in minions)
			{
				result += getMinionScore(minion) * Scale;
			}
			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}

		private int getMinionScore(Minion minion)
		{
			int result = 0;

			int minionHp = minion.Health;
			int minionAtk = minion.AttackDamage;

			result = minionAtk + minionHp;
			int baseValue = result;

			if (minion.IsFrozen)
			{
				return minionHp;
			}

			if (minion.HasTaunt)
			{
				result += 3;
			}

			if (minion.HasWindfury)
			{
				result += (int)(minionAtk * 0.5f);
			}

			if (minion.HasDivineShield)
			{
				result += (int)(baseValue * 1.5f);
			}

			if (minion.IsSilenced)
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
				result += (int)(baseValue * 1.5f);
			}

			return result;
		}
	}
}
