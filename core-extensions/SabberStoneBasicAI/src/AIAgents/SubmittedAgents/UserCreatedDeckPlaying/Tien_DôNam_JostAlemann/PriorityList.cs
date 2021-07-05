using System;
using System.Collections.Generic;

namespace SabberStoneBasicAI.AIAgents.c_isnt_sharp
{

    class Constants
    {
        public static List<string> Cards = new List<string>
        {
            "Sir Finley Mrrgglton",
            "Fiery War Axe",
            "Heroic Strike",
            "N'Zoth's First Mate",
            "Upgrade!",
            "Bloodsail Cultist",
            "Frothing Berserker",
            "Kor'kron Elite",
            "Arcanite Reaper",
            "Patches the Pirate",
            "Small-Time Buccaneer",
            "Southsea Deckhand",
            "Bloodsail Raider",
            "Southsea Captain",
            "Dread Corsair",
            "Naga Corsair",
            "The Coin"
        };

        public static List<double> PriorityWithWeapon = new List<double>
        {
            0.5, //"Sir Finley Mrrgglton",
            0.0, //"Fiery War Axe",
            0.5, //"Heroic Strike",
            0.0, //"N'Zoth's First Mate",
            0.8, //"Upgrade!",
            0.8, //"Bloodsail Cultist",
            0.5, //"Frothing Berserker",
            0.5, //"Kor'kron Elite",
            0.0, //"Arcanite Reaper",
            0.5, //"Patches the Pirate",
            0.8, //"Small-Time Buccaneer",
            0.8, //"Southsea Deckhand",
            0.8, //"Bloodsail Raider",
            0.5, //"Southsea Captain",
            0.8, //"Dread Corsair",
            0.8, //"Naga Corsair",
            1.0 //"The Coin"
        };

        public static List<double> PriorityNoWeapon = new List<double>
        {
            0.8, //"Sir Finley Mrrgglton",
            0.8, //"Fiery War Axe",
            0.8, //"Heroic Strike",
            0.8, //"N'Zoth's First Mate",
            0.1, //"Upgrade!",
            0.1, //"Bloodsail Cultist",
            0.8, //"Frothing Berserker",
            0.8, //"Kor'kron Elite",
            0.8, //"Arcanite Reaper",
            0.8, //"Patches the Pirate",
            0.2, //"Small-Time Buccaneer",
            0.1, //"Southsea Deckhand",
            0.1, //"Bloodsail Raider",
            0.8 ,//"Southsea Captain",
            0.0, //"Dread Corsair",
            0.3, //"Naga Corsair",
            1.0 //"The Coin"
        };

        public static List<string> HeroPowers = new List<string>
        {
            "Steady Shot",
            "Totemic Call",
            "Lesser Heal",
            "Shapeshift",
            "Reinforce",
            "Life Tap",
            "Fireblast",
            "Armor Up!",
            "Dagger Mastery"
        };

        public static List<double> PriorityHeroPowers = new List<double>
        {
            1.0, //"Steady Shot",
            0.1, //"Totemic Call",
            0.0, //"Lesser Heal",
            0.6, //"Shapeshift",
            0.1, //"Reinforce",
            0.2, //"Life Tap",
            0.0, //"Fireblast",
            0.1, //"Armor Up!",
            0.8, //"Dagger Mastery"
        };
    }

    class PriorityList
    {
        // each card has a value from 0 to 1
        public PriorityList(List<string> items, List<double> priorities)
        {
            this.Priorities = new Dictionary<string, double>();
            for(int i = 0; i < priorities.Count; i++)
            {
                this.Priorities[items[i]] = priorities[i];
            }
        }
        private Dictionary<string, double> Priorities;
        private Random Rnd = new Random();

        public string choose(List<string> cards)
        {
            // build an ascending list (e.g. [1, 1.5, 2.1, 3, 3.9])
            // the delta to the previous value is equal to the priority
            List<double> cumulative = new List<double>(cards.Count);
            double curr = 0;
            for(int i = 0; i < cards.Count; i++)
            {
                if (Priorities.ContainsKey(cards[i]))
                    curr += Priorities[cards[i]];
                else
                    Console.WriteLine($"unknown card: {cards[i]}");
                cumulative.Add(curr);
            }

            // choose a random card based on random number (e.g. 0 ... 3.9)
            double chosen = Rnd.NextDouble() * curr;
            for(int i = 0; i < cumulative.Count; i++)
            {
                if (chosen <= cumulative[i])
                    return cards[i];
            }

            Console.WriteLine("ERROR! CardPriorityList.choose could not found a valid card");
            return cards[0];
        }
    }
}