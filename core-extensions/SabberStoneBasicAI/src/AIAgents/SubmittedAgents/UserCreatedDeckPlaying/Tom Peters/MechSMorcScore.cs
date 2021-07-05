#region copyright
// SabberStone, Hearthstone Simulator in C# .NET Core
// Copyright (C) 2017-2019 SabberStone Team, darkfriend77 & rnilva
//
// SabberStone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License.
// SabberStone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model;
using SabberStoneBasicAI.Score;

namespace SabberStoneBasicAI.Score
{
	public class MechSMOrcScore : Score
	{
		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			int result = 0;

			//Mana Left
			result -= Controller.BaseMana - Controller.UsedMana;


			
			//Opponent has no board und you have
			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
			{
				result += 10;
			}

			//Heropower
			result += Controller.HeroPowerActivationsThisTurn;
			//Setting up Lethal
			if (MinionTotAtk >= OpHeroHp)
			{
		
				result += 15;
			}

			//Drawing more Cards through Jeeves
			if (Controller.NumCardsDrawnThisTurn >= 2)
			{
				
				result += 10;
			}

			//harder boardclear for the opponent
			result += (MinionTotHealth - OpMinionTotAtk)*2;

			//KILL the taunt minion
			result -= OpMinionTotHealthTaunt;

			// don't lose too many Minions
			result -= Controller.NumFriendlyMinionsThatDiedThisTurn ;

			//My Minions
			result += BoardZone.Count*3;
			result += MinionTotAtk*2;
			result += MinionTotHealth*1;

			//Op Minions
			result -= OpBoardZone.Count * 4;
			result -= OpMinionTotAtk * 3 ;
			result -= OpMinionTotHealth*2;
		
			//Having more Health then OPMinionsAttack
			if(OpMinionTotAtk + OpHeroAtk >= HeroHp )
			{
				result -= 100;
			}

			//Having more Health then OPMinionsAttack(+6 for safety)
			if (OpMinionTotAtk + OpHeroAtk+6 > HeroHp )
			{
				result -= 50;
			}

			// Attack!
			result += Controller.NumFriendlyMinionsThatAttackedThisTurn*2 ;

			//Going FACE!
			if(OpHeroHp <= 28)
			{
				result += 4;
			}
			if (OpHeroHp <= 26)
			{
				result += 4;
			}
			if (OpHeroHp <= 24)
			{
				result += 4;
			}
			if (OpHeroHp <= 22)
			{
				result += 4;
			}
			if (OpHeroHp <= 20)
			{
				result += 4;
			}
			if (OpHeroHp <= 18)
			{
				result += 4;
			}
			if (OpHeroHp <= 16)
			{
				result += 4;
			}
			if (OpHeroHp <= 14)
			{
				result += 4;
			}
			if (OpHeroHp <= 12)
			{
				result += 4;
			}
			if (OpHeroHp <= 10)
			{
				result += 4;
			}
			if (OpHeroHp <= 8)
			{
				result += 4;
			}
			if (OpHeroHp <= 6)
			{
				result += 4;
			}
			if (OpHeroHp <= 4)
			{
				result += 4;
			}
			if (OpHeroHp <= 2)
			{
				result += 4;
			}
			//Console.WriteLine("SCORE: " + result);
			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
