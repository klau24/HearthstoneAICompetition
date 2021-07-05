using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SabberStoneCore.Model.Zones;
using System.Threading.Tasks;
using System.Linq;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.Meta;
using SabberStoneCore.Model;
using SabberStoneCore.Actions;
using SabberStoneBasicAI.AIAgents.HenryChia.BoardScore;

namespace SabberStoneBasicAI.AIAgents.HenryChia
{
	class MCTS
	{
		public static double C = (1 / Math.Sqrt(2));
		public static int simulation_num = 1;

	//	public static List<TaskNode> new_nodes = new List<TaskNode>();
	//	public static List<TaskNode> new_error_nodes = new List<TaskNode>();

		public static POGame game_info;
		
		public static List<string> OpDeck;

		public static int num_my_board;
		public static int num_op_board;
		public static int num_my_hand;
		public static int num_op_hand;
		public static int num_my_hero;
		public static int num_op_hero;
		public static int num_my_deck;
		public static int num_op_deck;
		public static int num_remaining_mana;
		public static int num_my_attackdamage;
		public static int num_op_attackdamage;
		public static int num_my_minionHealth;
		public static int num_op_minionHealth;
		public static Score.Score MyAttSum = new MyAttackDamage();
		public static Score.Score OpAttSum = new OpAttackDamage();
		public static Score.Score myminiHealth = new MyMinionHealth();
		public static Score.Score opminiHealth = new OpMinionHealth();
		public static PlayerTask PlayMCTS(POGame game, double seconds)
		{		
			switch (game.CurrentPlayer.Opponent.HeroClass)
			{
				case SabberStoneCore.Enums.CardClass.SHAMAN:
					OpDeck = new List<string>()
					{
						"Tunnel Trogg",
						"Tunnel Trogg",
						"Totem Golem",
						"Totem Golem",
						"Thing from Below",
						"Thing from Below",
						"Spirit Claws",
						"Spirit Claws",
						"Maelstrom Portal",
						"Maelstrom Portal",
						"Lightning Storm",
						"Lightning Bolt",
						"Jade Lightning",
						"Jade Lightning",
						"Jade Claws",
						"Jade Claws",
						"Hex",
						"Hex",
						"Flametongue Totem",
						"Flametongue Totem",
						"Al'Akir the Windlord",
						"Patches the Pirate",
						"Small-Time Buccaneer",
						"Small-Time Buccaneer",
						"Bloodmage Thalnos",
						"Barnes",
						"Azure Drake",
						"Azure Drake",
						"Aya Blackpaw",
						"Ragnaros the Firelord"
					};
					break;
				case SabberStoneCore.Enums.CardClass.WARRIOR:
					OpDeck = new List<string>()
					{
						"Sir Finley Mrrgglton",
						"Fiery War Axe",
						"Fiery War Axe",
						"Heroic Strike",
						"Heroic Strike",
						"N'Zoth's First Mate",
						"N'Zoth's First Mate",
						"Upgrade!",
						"Upgrade!",
						"Bloodsail Cultist",
						"Bloodsail Cultist",
						"Frothing Berserker",
						"Frothing Berserker",
						"Kor'kron Elite",
						"Kor'kron Elite",
						"Arcanite Reaper",
						"Arcanite Reaper",
						"Patches the Pirate",
						"Small-Time Buccaneer",
						"Small-Time Buccaneer",
						"Southsea Deckhand",
						"Southsea Deckhand",
						"Bloodsail Raider",
						"Bloodsail Raider",
						"Southsea Captain",
						"Southsea Captain",
						"Dread Corsair",
						"Dread Corsair",
						"Naga Corsair",
						"Naga Corsair",
					};
					break;
				case SabberStoneCore.Enums.CardClass.MAGE:
					OpDeck = new List<string>()
					{
						"Forbidden Flame",
						"Arcane Blast",
						"Babbling Book",
						"Frostbolt",
						"Arcane Intellect",
						"Forgotten Torch",
						"Ice Barrier",
						"Ice Block",
						"Manic Soulcaster",
						"Volcanic Potion",
						"Fireball",
						"Polymorph",
						"Water Elemental",
						"Cabalist's Tome",
						"Blizzard",
						"Firelands Portal",
						"Flamestrike",
						"Acidic Swamp Ooze",
						"Bloodmage Thalnos",
						"Dirty Rat",
						"Doomsayer",
						"Brann Bronzebeard",
						"Kabal Courier",
						"Mind Control Tech",
						"Kazakus",
						"Refreshment Vendor",
						"Azure Drake",
						"Reno Jackson",
						"Sylvanas Windrunner",
						"Alexstrasza"
					};
					break;
				case SabberStoneCore.Enums.CardClass.WARLOCK:
					OpDeck = new List<string>()
					{
						"Flame Imp",
						"Flame Imp",
						"Malchezaar's Imp",
						"Malchezaar's Imp",
						"Possessed Villager",
						"Possessed Villager",
						"Soulfire",
						"Soulfire",
						"Voidwalker",
						"Voidwalker",
						"Dark Peddler",
						"Dark Peddler",
						"Darkshire Librarian",
						"Darkshire Librarian",
						"Darkshire Councilman",
						"Darkshire Councilman",
						"Imp Gang Boss",
						"Imp Gang Boss",
						"Silverware Golem",
						"Silverware Golem",
						"Doomguard",
						"Doomguard",
						"Abusive Sergeant",
						"Crazed Alchemist",
						"Dire Wolf Alpha",
						"Dire Wolf Alpha",
						"Knife Juggler",
						"Knife Juggler",
						"Defender of Argus",
						"Defender of Argus"
					};
					break;
				case SabberStoneCore.Enums.CardClass.PALADIN:
					OpDeck = new List<string>()
					{
						"Smuggler's Run",
						"Smuggler's Run",
						"Argent Lance",
						"Grimestreet Outfitter",
						"Grimestreet Outfitter",
						"Aldor Peacekeeper",
						"Aldor Peacekeeper",
						"Wickerflame Burnbristle",
						"Truesilver Champion",
						"Truesilver Champion",
						"Grimestreet Enforcer",
						"Grimestreet Enforcer",
						"Tirion Fordring",
						"Sir Finley Mrrgglton",
						"Worgen Infiltrator",
						"Worgen Infiltrator",
						"Flame Juggler",
						"Flame Juggler",
						"Acolyte of Pain",
						"Acolyte of Pain",
						"Argent Horserider",
						"Argent Horserider",
						"Sen'jin Shieldmasta",
						"Sen'jin Shieldmasta",
						"Psych-o-Tron",
						"Second-Rate Bruiser",
						"Second-Rate Bruiser",
						"Argent Commander",
						"Argent Commander",
						"Don Han'Cho"
					};
					break;
				case SabberStoneCore.Enums.CardClass.ROGUE:
					OpDeck = new List<string>()
					{
							"Backstab",
							"Backstab",
							"Counterfeit Coin",
							"Preparation",
							"Preparation",
							"Cold Blood",
							"Cold Blood",
							"Conceal",
							"Conceal",
							"Swashburglar",
							"Eviscerate",
							"Eviscerate",
							"Sap",
							"Sap",
							"Edwin VanCleef",
							"Fan of Knives",
							"Fan of Knives",
							"Tomb Pillager",
							"Tomb Pillager",
							"Patches the Pirate",
							"Small-Time Buccaneer",
							"Small-Time Buccaneer",
							"Bloodmage Thalnos",
							"Questing Adventurer",
							"Questing Adventurer",
							"Azure Drake",
							"Azure Drake",
							"Leeroy Jenkins",
							"Gadgetzan Auctioneer",
							"Gadgetzan Auctioneer"
					};
					break;
				default:

					break;
			}
			



			game_info = game;
			DateTime start = DateTime.Now;
			TaskNode root = new TaskNode(null, null, game.getCopy());
			root.TotNumVisits = 0;

			while (true)
			{

				TaskNode node = root.SelectNode();
				if (TimeUp(start, seconds)) break;

				node = node.Expand();
				if (TimeUp(start, seconds)) break;

				
				double r = node.SimulateGames(simulation_num);
				if (TimeUp(start, seconds)) break;
				
				r = (double)r / (double)simulation_num;
				node.Backpropagate(r);
				if (TimeUp(start, seconds)) break;



			}

			TaskNode best = null;

			foreach (TaskNode child in root.Children)
			{

				if (best == null || child.TotNumVisits > best.TotNumVisits || (child.TotNumVisits == best.TotNumVisits && child.Wins > best.Wins))
				{
					best = child;
				}
			}
		
			
			return best.Action;


		}


		private static bool TimeUp(DateTime start, double seconds)
		{
			return (DateTime.Now - start).TotalSeconds > seconds;
		}

		public class TaskNode
		{
			static Random rand = new Random();
			POGame Game = null;
			TaskNode Parent = null;
			public List<PlayerTask> PossibleActions = null;
			public static object lockey = new object();
			public PlayerTask Action { get; private set; } = null;
			public List<TaskNode> Children { get; private set; } = null;

			public List<string>MyHandCard { get; private set; } = null;

			public List<string> MyDeckCard { get; private set; } = null;

			public List<string> OpHandCardAndDeck { get; private set; } = null;

			public int TotNumVisits { get; set; } = 0; 
			public double Wins { get; private set; } = 0; 

			public TaskNode(TaskNode parent, PlayerTask action, POGame game)
			{
				Game = game;
				Parent = parent;
				Action = action;
				PossibleActions = Game.CurrentPlayer.Options();
				Children = new List<TaskNode>();
				MyHandCard = new List<string>();
				MyDeckCard = new List<string>();
				OpHandCardAndDeck = new List<string>();
			}

			public TaskNode SelectNode()
			{
				if (PossibleActions.Count == 0 && Children.Count > 0)
				{
					double candidateScore = Double.MinValue;

					TaskNode candidate = null;

					foreach (TaskNode child in Children)
					{
						if (child.TotNumVisits <= 1)
						{
							candidateScore = Double.MaxValue;
							candidate = child;
						}
						else
						{
							double childScore = child.UCB1Score();
							if (childScore > candidateScore)
							{
								candidateScore = childScore;
								candidate = child;
							}
						}

					}

					return candidate.SelectNode();

				}

				return this;

			}

			private double UCB1Score()
			{		
				double exploitScore = (double)Wins / (double)TotNumVisits;
				double explorationScore = Math.Sqrt(Math.Log(Parent.TotNumVisits) / TotNumVisits);
				explorationScore *= C;
				return exploitScore + explorationScore;
				
			}

			public TaskNode Expand()
			{
				if (PossibleActions.Count == 0 && Children.Count < 1)
				{
					return this;
				}
				else
				{
					var myPlayer = game_info.CurrentPlayer;
					var opponentPlayer = game_info.CurrentPlayer.Opponent;
					POGame poGameFailed = Game.getCopy();
					List<PlayerTask> validOptions = new List<PlayerTask>();
					
					foreach (var opt in PossibleActions.ToList())
					{
						var d = Game.Simulate(new List<PlayerTask> { opt });
						if (d[opt] != null)
						{
							validOptions.Add(opt);
						}
						else
						{
							PossibleActions.Remove(opt);
						}
					}

					if (validOptions.Count != 0)
					{
						int r = rand.Next(0, validOptions.Count - 1);
						PlayerTask act = validOptions[r];
						Dictionary<PlayerTask, POGame> dic = Game.Simulate(new List<PlayerTask> { act });
						POGame childGame = dic[act];
						TaskNode child = new TaskNode(this, act, childGame);
						this.Children.Add(child);
						
						PossibleActions.Remove(act);

						//my node
						if (childGame.CurrentPlayer.PlayerId == myPlayer.PlayerId)
						{
							//MyHandCard
							foreach (IPlayable card in childGame.CurrentPlayer.HandZone.GetAll())
							{
								if (card.Card.Name != "No Way!")
								{
									child.MyHandCard.Add(card.Card.Name);
								}															
							}
							
							//MyDeckCard
							foreach (IPlayable card in childGame.CurrentPlayer.DeckZone.GetAll())
							{
								child.MyDeckCard.Add(card.Card.Name);
							}

							//OpHandCardAndDeck							
							List<string> OpCardKnown = new List<string>();
							foreach (IPlayable card in childGame.CurrentPlayer.Opponent.GraveyardZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}
							foreach (IPlayable card in childGame.CurrentPlayer.Opponent.BoardZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}
							foreach (IPlayable card in childGame.CurrentPlayer.Opponent.SecretZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}
					
							var OpponentDeck = OpDeck.ToList();
							foreach (var cardKnown in OpCardKnown)
							{
								if (OpDeck.Contains(cardKnown))
								{
									OpponentDeck.Remove(cardKnown);
								}
							}
							child.OpHandCardAndDeck = OpponentDeck.ToList();


						}

						//op node
						else if (childGame.CurrentPlayer.PlayerId == opponentPlayer.PlayerId)
						{
							//MyHandCard
							foreach (IPlayable card in childGame.CurrentPlayer.Opponent.HandZone.GetAll())
							{
								if (card.Card.Name != "No Way!")
								{
									child.MyHandCard.Add(card.Card.Name);
								}							
							}
							
							//MyDeckCard
							foreach (IPlayable card in childGame.CurrentPlayer.Opponent.DeckZone.GetAll())
							{
								child.MyDeckCard.Add(card.Card.Name);
							}

							//OpHandCardAndDeck
							List<string> OpCardKnown = new List<string>();
							foreach (IPlayable card in childGame.CurrentPlayer.GraveyardZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}
							foreach (IPlayable card in childGame.CurrentPlayer.BoardZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}
							foreach (IPlayable card in childGame.CurrentPlayer.SecretZone.GetAll())
							{
								OpCardKnown.Add(card.Card.Name);
							}

							var OpponentDeck = OpDeck.ToList();
							foreach (var cardKnown in OpCardKnown)
							{
								if (OpDeck.Contains(cardKnown))
								{
									OpponentDeck.Remove(cardKnown);
								}
							}
							child.OpHandCardAndDeck = OpponentDeck.ToList();
						}

						return child;
					}
					else
					{
						return this;
					}

				}

			}

			public double SimulateGames(int numGames)
			{
				double wins = 0;
				for (int i = 0; i < numGames; ++i)
				{
					wins += Simulate();
				}
				return wins;
			}

			private double Simulate()
			{
				

				var myPlayer = game_info.CurrentPlayer;
				var opponentPlayer = game_info.CurrentPlayer.Opponent;
				bool simulateERROR = false;
				int count=0;
				POGame gameClone = Game.getCopy();
			
				int initialPlayer = gameClone.CurrentPlayer.PlayerId;

				int turn = 1;
				var childgame_OpHandCardAndDeck = OpHandCardAndDeck.ToList();
				var childgame_MyDeckCard = MyDeckCard.ToList();

				//Initial setting
				//my node
				try
				{
	
					if (gameClone.CurrentPlayer.PlayerId == myPlayer.PlayerId)
					{					
						int num_MyHandCard = gameClone.CurrentPlayer.HandZone.Count;
						int num_OpHandCard = gameClone.CurrentPlayer.Opponent.HandZone.Count;

						//myhandcard				
						for (int i = 0; i < num_MyHandCard; i++)
						{

							gameClone.CurrentPlayer.HandZone.Remove(gameClone.CurrentPlayer.HandZone[0]);

						}
						for (int i = 0; i < num_OpHandCard; i++)
						{

							gameClone.CurrentPlayer.Opponent.HandZone.Remove(gameClone.CurrentPlayer.Opponent.HandZone[0]);

						}

						foreach (var card in MyHandCard)
						{
							Card cardcopy = Cards.FromName(card);
							IPlayable AddCard;
							if (cardcopy != null)
								AddCard = Generic.DrawCard(gameClone.CurrentPlayer, cardcopy);
						}

						//ophandcard	
						for (int c = 0; c < num_OpHandCard; c++)
						{
							int r = rand.Next(0, childgame_OpHandCardAndDeck.Count - 1);
							IPlayable AddCard = Generic.DrawCard(gameClone.CurrentPlayer.Opponent, Cards.FromName(childgame_OpHandCardAndDeck[r]));
							childgame_OpHandCardAndDeck.Remove(childgame_OpHandCardAndDeck[r]);
						}
					}

					//op node
					else if (gameClone.CurrentPlayer.PlayerId == opponentPlayer.PlayerId)
					{
						int num_MyHandCard = gameClone.CurrentPlayer.Opponent.HandZone.Count;
						int num_OpHandCard = gameClone.CurrentPlayer.HandZone.Count;


						//myhandcard				
						for (int i = 0; i < num_MyHandCard; i++)
						{

							gameClone.CurrentPlayer.Opponent.HandZone.Remove(gameClone.CurrentPlayer.Opponent.HandZone[0]);

						}
						for (int i = 0; i < num_OpHandCard; i++)
						{

							gameClone.CurrentPlayer.HandZone.Remove(gameClone.CurrentPlayer.HandZone[0]);

						}
						foreach (var card in MyHandCard)
						{
							Card cardcopy = Cards.FromName(card);
							IPlayable AddCard;
							if (cardcopy != null)
								AddCard = Generic.DrawCard(gameClone.CurrentPlayer.Opponent, cardcopy);
						}

						//ophandcard				
						for (int c = 0; c < num_OpHandCard; c++)
						{
							int r = rand.Next(0, childgame_OpHandCardAndDeck.Count - 1);
							IPlayable AddCard = Generic.DrawCard(gameClone.CurrentPlayer, Cards.FromName(childgame_OpHandCardAndDeck[r]));
							childgame_OpHandCardAndDeck.Remove(childgame_OpHandCardAndDeck[r]);
						}
					}


			
					while (true)
					{
						POGame poGameFailed = gameClone;

						//end game
						if (gameClone.State == SabberStoneCore.Enums.State.COMPLETE)
						{
							if (simulateERROR == true)
							{
								return 0;
							}

							//me
							if (gameClone.CurrentPlayer.PlayerId == myPlayer.PlayerId)
							{
								//I win
								if (gameClone.CurrentPlayer.PlayState == SabberStoneCore.Enums.PlayState.WON)
								{
									return 1;
								}

								//I lose
								else if (gameClone.CurrentPlayer.PlayState == SabberStoneCore.Enums.PlayState.LOST)
								{
									return 0;
								}
							}

							//opponent
							else if (gameClone.CurrentPlayer.PlayerId == opponentPlayer.PlayerId)
							{
								//opponent win
								if (gameClone.CurrentPlayer.PlayState == SabberStoneCore.Enums.PlayState.WON)
								{
									return 0;
								}

								//opponent lose
								else if (gameClone.CurrentPlayer.PlayState == SabberStoneCore.Enums.PlayState.LOST)
								{
									return 1;
								}
								

							}
						
						}

						#region 
						//gp
						List<PlayerTask> options = gameClone.CurrentPlayer.Options();
						List<PlayerTask> validOptions = new List<PlayerTask>();
						foreach (var opt in options)
						{
							var d = gameClone.Simulate(new List<PlayerTask> { opt });
							if (d[opt] != null) validOptions.Add(opt);
						}
						PlayerTask best = options[0];
						double candidateScore = Double.MaxValue;

						TreeScore.tree_node = null;
						switch (gameClone.CurrentPlayer.HeroClass)
						{
							case SabberStoneCore.Enums.CardClass.WARRIOR:
								TreeScore.tree_node = "khiBDEFgFEafEAhfDkDBCC";
								break;						

							default:
								TreeScore.tree_node = "kaeHDimBcDCBjB";
								break;
						}


						double score = Double.MaxValue;
						foreach (PlayerTask task in validOptions)
						{


							if (task.PlayerTaskType != PlayerTaskType.CONCEDE && task.PlayerTaskType != PlayerTaskType.END_TURN)
							{
								Dictionary<PlayerTask, POGame> dic = gameClone.Simulate(new List<PlayerTask> { task });
								POGame gameC = dic[task];


								if (gameC != null)
								{
									Controller my = gameC.CurrentPlayer;
									Controller op = gameC.CurrentPlayer.Opponent;
									MyAttSum.Controller = my;
									OpAttSum.Controller = my;
									myminiHealth.Controller = my;
									opminiHealth.Controller = my;
									
									num_my_board = my.BoardZone.Count;
									num_op_board = op.BoardZone.Count;
									num_my_hand = my.HandZone.Count;
									num_op_hand = op.HandZone.Count;
									num_my_hero = my.Hero.Health + my.Hero.Armor;
									num_op_hero = op.Hero.Health + op.Hero.Armor;
									num_my_deck = my.DeckZone.Count;
									num_op_deck = op.DeckZone.Count;
									num_remaining_mana = my.RemainingMana;
									num_my_attackdamage = MyAttSum.Rate();
									num_op_attackdamage = OpAttSum.Rate();
									num_my_minionHealth = myminiHealth.Rate();
									num_op_minionHealth = opminiHealth.Rate();

									score = Math.Round(TreeScore.Node_Evaluation(num_my_board, num_op_board, num_my_hand, num_op_hand,
							num_my_hero, num_op_hero, num_my_deck, num_op_deck, num_remaining_mana, num_my_attackdamage, num_op_attackdamage,
							num_my_minionHealth, num_op_minionHealth), 15, MidpointRounding.AwayFromZero);


								}
								else
								{
									score = Double.MaxValue;
								}

								if (num_my_hero < 1)
								{
									score = Double.MaxValue;
								}

								if (num_op_hero < 1)
								{
									score = Double.MinValue;
								}


								if (score < candidateScore)
								{
									candidateScore = score;
									best = task;
								}

							}
						}


						if (best == null)
						{
							best = options[0];
						}

						validOptions.Clear();

						int draw_count = 0;
						while (gameClone.CurrentPlayer.HandZone.Count > 0 &&
							gameClone.CurrentPlayer.HandZone.Last().Card.Name == "No Way!")
						{
							gameClone.CurrentPlayer.HandZone.Remove(gameClone.CurrentPlayer.HandZone.Last());
							draw_count++;

						}
						for (int i = 0; i < draw_count; i++)
						{
							if (gameClone.CurrentPlayer.PlayerId == myPlayer.PlayerId && childgame_MyDeckCard.Count > 0)
							{
								int draw = rand.Next(0, childgame_MyDeckCard.Count - 1);
								IPlayable AddCard = Generic.DrawCard(gameClone.CurrentPlayer, Cards.FromName(childgame_MyDeckCard[draw]));
								childgame_MyDeckCard.Remove(childgame_MyDeckCard[draw]);
							}
							else if (gameClone.CurrentPlayer.PlayerId == opponentPlayer.PlayerId && childgame_OpHandCardAndDeck.Count > 0)
							{
								int draw = rand.Next(0, childgame_OpHandCardAndDeck.Count - 1);
								IPlayable AddCard = Generic.DrawCard(gameClone.CurrentPlayer, Cards.FromName(childgame_OpHandCardAndDeck[draw]));
								childgame_OpHandCardAndDeck.Remove(childgame_OpHandCardAndDeck[draw]);
							}
						}



						if (gameClone.CurrentPlayer.BoardZone.Count == 0 &&
							gameClone.CurrentPlayer.Opponent.BoardZone.Count == 0)
						{
							count++;
						}
						if (count >= 20)
						{
							simulateERROR = true;
						}




						Dictionary<PlayerTask, POGame> dic_final = gameClone.Simulate(new List<PlayerTask> { best });
						gameClone = dic_final[best];
						#endregion
					}
				}
				catch (Exception e)
				{
					return 0;
				}
					
			}


			public void Backpropagate(double score)
			{
				TaskNode node = this;
				while (node.Parent != null)
				{
					node.UpdateScore(score);
					node = node.Parent;
				}
				node.TotNumVisits++;
			}

			private void UpdateScore(double score)
			{
				TotNumVisits++;
				Wins += score;
			}

		}

	}
}
