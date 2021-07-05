using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Model.Entities;

namespace SabberStoneAICompetition.src.AIAgents
{



	public interface IScore {
		Controller Controller { get; set; }
		Func<List<IPlayable>, List<int>> MulliganRule();
		int Rate();

		int MyRate();

}

	public abstract class MyScoreThoms : IScore
	{
		public Controller Controller { get; set; }
		public int MyHp => Controller.Hero.Health+Controller.Hero.Armor;
		public int MyArmor => Controller.Hero.Armor;
		public int OpHp => Controller.Opponent.Hero.Health + Controller.Opponent.Hero.Armor;
		public int OpArmor => Controller.Opponent.Hero.Armor;

		public int MyAtk => Controller.Hero.TotalAttackDamage;
		public int OpAtk => Controller.Opponent.Hero.TotalAttackDamage;


		public BoardZone MyBoardZone => Controller.BoardZone;
		public BoardZone OpBoardZone => Controller.Opponent.BoardZone;

		public int MyMinionTotHealth => MyBoardZone.Sum(p => p.Health);
		public int OpMinionTotHealth => OpBoardZone.Sum(p => p.Health);
		public int MyMinionTotAtk => MyBoardZone.Sum(p => p.AttackDamage);
		public int OpMinionTotAtk => OpBoardZone.Sum(p => p.AttackDamage);
		public int MyMinionTotHealthTaunt => MyBoardZone.Where(p => p.HasTaunt).Sum(p => p.Health);
		public int OpMinionTotHealthTaunt => OpBoardZone.Where(p => p.HasTaunt).Sum(p => p.Health);
		public int MyMinionTotHealthDeathRattle => MyBoardZone.Where(p => p.HasDeathrattle).Sum(p => p.Health);
		public int OpMinionTotHealthDeathrattle => OpBoardZone.Where(p => p.HasDeathrattle).Sum(p => p.Health);
		public int MyMinionTotHealthDivineShield => MyBoardZone.Where(p => p.HasDivineShield).Sum(p => p.Health);
		public int OpMinionTotHealthDivineShield => OpBoardZone.Where(p => p.HasDivineShield).Sum(p => p.Health);


		public HandZone Hand => Controller.HandZone;
		public int HandTotCost => Hand.Sum(p => p.Cost);
		public int MyTotalHandCount => Controller.HandZone.Count;
		public int OpHandCount => Controller.Opponent.HandZone.Count;


		public int MyDeckCnt => Controller.DeckZone.Count;
		public int OpDeckCnt => Controller.Opponent.DeckZone.Count;




		public virtual Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => new List<int>();
		}

		public virtual int MyRate()
		{
			throw new NotImplementedException();
		}



		public virtual int Rate()
		{
			return 0;
		}


	}
}
