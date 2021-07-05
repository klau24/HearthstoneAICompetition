using System;
using System.Collections.Generic;
using SabberStoneCore.Model.Entities;
using System.Linq;


namespace SabberStoneBasicAI.AIAgents.BotterThanYouThink
{
	public class CustomAggroScoreHempel : Score.Score
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

			Controller player = Controller;
			Controller opp = player.Opponent;

			int heroHp = HeroHp + player.Hero.Armor;
			int oppHeroHp = OpHeroHp + opp.Hero.Armor;
			result += (heroHp - oppHeroHp) * 2 * Scale;
			result += player.NumCardsDrawnThisTurn * Scale;
			result += player.NumDiscardedThisGame * -5 * Scale;
			result += player.NumFriendlyMinionsThatAttackedThisTurn * Scale;
			result += player.NumMinionsPlayerKilledThisTurn * Scale;
			result += player.RemainingMana * -Scale;

			Minion[] minions = BoardZone.GetAll();
			Minion[] oppMinions = OpBoardZone.GetAll();
			result += (minions.Count() - oppMinions.Count()) * 3 * Scale;
			result += getTotalMinionScore(minions);
			result -= getTotalMinionScore(oppMinions);

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
			return p => p.Where(t => t.Cost > 2).Select(t => t.Id).ToList();
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
				result += 4;
			}

			if (minion.HasWindfury)
			{
				result += (int)(minionAtk * 0.5f);
			}

			if (minion.HasDivineShield)
			{
				result += (int)(baseValue * 2);
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
