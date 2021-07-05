using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Enums;

namespace SabberStoneBasicAI.Score.mudgal_kumar_2
{
    class WeightedScore: Score{
        List<Tuple<int,double>> weightedFeatures;

        public override int Rate(){
            weightedFeatures = new List<Tuple<int, double>>();
            weightedFeatures.Add(new Tuple<int, double>(HeroHp,4));
            weightedFeatures.Add(new Tuple<int, double>(HeroAtk,1.1));
            weightedFeatures.Add(new Tuple<int, double>(Controller.Hero.Armor,3.5));
            weightedFeatures.Add(new Tuple<int, double>(HandCnt,3));
            weightedFeatures.Add(new Tuple<int, double>(DeckCnt,3));
            weightedFeatures.Add(new Tuple<int, double>(MinionTotHealth,3));
            weightedFeatures.Add(new Tuple<int, double>(MinionTotAtk,2));
            weightedFeatures.Add(new Tuple<int, double>(MinionTotHealthTaunt,3));

            weightedFeatures.Add(new Tuple<int, double>(OpHeroHp,-5));
            weightedFeatures.Add(new Tuple<int, double>(OpHeroAtk,-1.2));
            weightedFeatures.Add(new Tuple<int, double>(Controller.Opponent.Hero.Armor,-4.5));
            weightedFeatures.Add(new Tuple<int, double>(OpHandCnt,-4));
            weightedFeatures.Add(new Tuple<int, double>(OpDeckCnt,-4));
            weightedFeatures.Add(new Tuple<int, double>(OpMinionTotHealth,-3.1));
            weightedFeatures.Add(new Tuple<int, double>(OpMinionTotAtk,-2.1));
            weightedFeatures.Add(new Tuple<int, double>(OpMinionTotHealthTaunt,-4));


            foreach (var minion in BoardZone)
			{
                weightedFeatures.Add(new Tuple<int, double>(minion.Health,1));
                weightedFeatures.Add(new Tuple<int, double>(minion.AttackDamage,1));
			}

			foreach (var minion in OpBoardZone)
			{
                weightedFeatures.Add(new Tuple<int, double>(minion.Health,1));
                weightedFeatures.Add(new Tuple<int, double>(minion.AttackDamage,1));
			}

            int score = 0;
            if (OpHeroHp< 1){
                return Int32.MaxValue;
            }
            if(HeroHp < 1){
                return Int32.MinValue;
            }
            double result = 0.0;

            result = weightedFeatures.Select((pair)=>pair.Item1*pair.Item2).Sum();
            score = (int)Math.Round(result);
            return score;
        }

        public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
    }
}
