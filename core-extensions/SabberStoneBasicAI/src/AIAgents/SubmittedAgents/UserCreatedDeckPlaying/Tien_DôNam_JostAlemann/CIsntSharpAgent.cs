using System.Linq;
using System;
using System.Collections.Generic;
using SabberStoneBasicAI.Meta;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;


// TODO choose your own namespace by setting up <submission_tag>
// each added file needs to use this namespace or a subnamespace of it

// WE ARE BACHELOR STUDENTS
// Jost Alemann
// Tien Do Nam
namespace SabberStoneBasicAI.AIAgents.c_isnt_sharp
{
	class CIsntSharpAgent : AbstractAgent
	{

		/*
		specify Deck to play:
		MiraclePirateRogue, ZooDiscardWarlock, RenoKazakusDragonPriest, MidrangeSecretHunter, MidrangeBuffPaladin, MurlocDruid, MidrangeJadeShaman, AggroPirateWarrior, RenoKazakusMage
		
		WE CHOSE AggroPirateWarrior
		*/

		/*
		specify Hero Class to play:
		MAGE, ROGUE, PRIEST, PALADIN, WARRIOR, WARLOCK, SHAMAN, DRUID, HUNTER

		WE CHOSE WARRIOR
		*/

		// TODO: Mulligan -> dont keep duplicates, Use coin smart, Use Berserker smarter when trading

		private PriorityList CardPrioritiesWithWeapon;
		private PriorityList CardPrioritiesNoWeapon;
		private PriorityList HeroPowerPriorities;

		private bool SelectHeroPower = false;
		private static int counter = 0;

		public CIsntSharpAgent()
		{
			base.preferedDeck = Decks.AggroPirateWarrior;
			base.preferedHero = CardClass.WARRIOR;
		}

		public override PlayerTask GetMove(POGame game)
		{

			if (SelectHeroPower)
			{
				SelectHeroPower = false;
				List<IPlayable> choices = game.CurrentPlayer.Choice.Choices.Select(id => game.getGame().IdEntityDic[id]).ToList();
				string selected = HeroPowerPriorities.choose(choices.Select(c => c.Card.Name).Distinct().ToList());
				return ChooseTask.Pick(game.CurrentPlayer, choices.First(c => c.Card.Name == selected).Id);
			}

			int mana = game.CurrentPlayer.BaseMana + game.CurrentPlayer.TemporaryMana - game.CurrentPlayer.UsedMana;
			List<PlayerTask> options = game.CurrentPlayer.Options();
			List<PlayerTask> heroPowers = options.Where(t => t.PlayerTaskType == PlayerTaskType.HERO_POWER).ToList();
			List<PlayerTask> playCards = options.Where(t => t.PlayerTaskType == PlayerTaskType.PLAY_CARD && t.Source.Card.Cost <= mana).ToList();
			List<Minion> ourMinionsReady = game.Minions.Where(m => m.Controller.Id == game.CurrentPlayer.Id && m.CanAttack).ToList();

			if (game.CurrentPlayer.MulliganState == Mulligan.INPUT)
			{
				// mulligan
				List<IPlayable> choices = game.CurrentPlayer.Choice.Choices.Select(id => game.getGame().IdEntityDic[id]).ToList();
				choices = choices.Where(c => c.Cost <= 3 && c.Card.Name != "Patches the Pirate").ToList();
                return ChooseTask.Mulligan(game.CurrentPlayer, choices.Select(c => c.Id).ToList());
			}
			else if (playCards.Count != 0)
			{
				// play card
				List<string> cardNames = playCards.Select(c => c.Source.Card.Name).Distinct().ToList();

				string selectedCard;
				if (game.CurrentPlayer.Hero.Weapon != null)
					// we have a weapon
					selectedCard = CardPrioritiesWithWeapon.choose(cardNames);
				else
					// we don't have a weapon
					selectedCard = CardPrioritiesNoWeapon.choose(cardNames);

				if (selectedCard == "Sir Finley Mrrgglton")
					SelectHeroPower = true;
				
				return playCards.First(t => t.Source.Card.Name == selectedCard);
			}
			else if (game.CurrentPlayer.Hero.CanAttack)
			{
				// hero attack
                List<Minion> enemyMinions = game.Minions.Where(m => m.Controller.Id == game.CurrentOpponent.Id).ToList();
                List<Minion> enemyTaunts = enemyMinions.Where(m => m.HasTaunt == true).ToList();

                if (enemyTaunts.Count != 0)
					return HeroAttackTask.Any(game.CurrentPlayer, enemyTaunts[0]);
                else
					return HeroAttackTask.Any(game.CurrentPlayer, game.CurrentOpponent.Hero);
			}
			else if (ourMinionsReady.Count != 0)
			{
                // minion attack
                List<Minion> enemyMinions = game.Minions.Where(m => m.Controller.Id == game.CurrentOpponent.Id).ToList();
                List<Minion> enemyTaunts = enemyMinions.Where(m => m.HasTaunt == true).ToList();

                if (enemyTaunts.Count != 0)
                {
                    int tauntHealth = enemyTaunts[0].Health;
                    int tauntAttack = enemyTaunts[0].AttackDamage;

					// perfectTraders: survive the attack, kill the Taunt and don't have more overkill/wasted damage than 2
                    List<Minion> perfectTraders = ourMinionsReady.Where(m => m.Health > tauntAttack && m.AttackDamage >= tauntHealth && m.AttackDamage <= (tauntHealth+2)).ToList();

					// almost perfect Traders: survive the attack and kill the Taunt
                    List<Minion> almostPerfectTraders = ourMinionsReady.Where(m => m.Health > tauntAttack && m.AttackDamage >= tauntHealth).ToList();

                    // goodTraders: kill Taunt
                    List<Minion> goodTraders = ourMinionsReady.Where(m => m.AttackDamage > tauntHealth).ToList();

					// survivingTraders: survive an attack but don't necessarily kill the taunt
                    List<Minion> survivingTraders = ourMinionsReady.Where(m => m.Health > tauntAttack).ToList();

                    // trade perfect
                    if (perfectTraders.Count != 0)
                    {
						perfectTraders = perfectTraders.OrderBy(m => m.Health + m.AttackDamage).ToList();
                        return MinionAttackTask.Any(game.CurrentPlayer, perfectTraders[0], enemyTaunts[0]);
                    }

                    // trade almost perfect
                    else if (almostPerfectTraders.Count != 0)
                    {
                        almostPerfectTraders = almostPerfectTraders.OrderBy(m => m.Health + m.AttackDamage).ToList();
                        return MinionAttackTask.Any(game.CurrentPlayer, almostPerfectTraders[0], enemyTaunts[0]);
                    }

                    // trade good
                    else if (goodTraders.Count != 0)
                    {
                        // sort good traders and choose weakest to not waste potential
                        goodTraders = goodTraders.OrderBy(m => m.Health + m.AttackDamage).ToList();
                        return MinionAttackTask.Any(game.CurrentPlayer, goodTraders[0], enemyTaunts[0]);
                    }

                    // trade so that minions survive
                    else if (survivingTraders.Count != 0)
                        return MinionAttackTask.Any(game.CurrentPlayer, survivingTraders[0], enemyTaunts[0]);

                    // trade random
                    else
                        return MinionAttackTask.Any(game.CurrentPlayer, ourMinionsReady[0], enemyTaunts[0]);
                }
                else
                    return MinionAttackTask.Any(game.CurrentPlayer, ourMinionsReady[0], game.CurrentOpponent.Hero);
            }
			else if (mana >= 2 && heroPowers.Count != 0)
			{
				// hero power
				if (game.CurrentPlayer.Hero.HeroPower.Card.Name == "Lesser Heal" || game.CurrentPlayer.Hero.HeroPower.Card.Name == "Fireblast")
					return options[0]; // end turn, because we don't know how to set the target
				else
					return heroPowers[0]; // use hero power
			}
			else
			{
				// fallback: end turn
				return options[0];
			}
		}

		// called before each match
		public override void InitializeAgent()
		{
			CardPrioritiesWithWeapon = new PriorityList(Constants.Cards, Constants.PriorityWithWeapon);
			CardPrioritiesNoWeapon = new PriorityList(Constants.Cards, Constants.PriorityNoWeapon);
			HeroPowerPriorities = new PriorityList(Constants.HeroPowers, Constants.PriorityHeroPowers);
		}

		public override void FinalizeAgent()
		{
			// this is never called?
		}
		
		// called before each match
		public override void InitializeGame()
		{
		}

		// called after each match
		public override void FinalizeGame()
		{
			counter++;
            if (counter % 50 == 0)
				Console.WriteLine(counter);
        }
	}
}
