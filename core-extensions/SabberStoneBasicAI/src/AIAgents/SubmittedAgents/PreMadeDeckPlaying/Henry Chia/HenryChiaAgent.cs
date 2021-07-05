using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneBasicAI.AIAgents.HenryChia
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using SabberStoneCore.Enums;
	using SabberStoneBasicAI.AIAgents;
	using SabberStoneCore.Tasks.PlayerTasks;
	using SabberStoneBasicAI.PartialObservation;
	using SabberStoneCore.Model.Entities;
	using SabberStoneBasicAI.Score;
	using SabberStoneBasicAI.AIAgents.HenryChia.BoardScore;

	//MctsAgent & GeneticProgrammingAgent by HenryChia
	class HenryChiaAgent : AbstractAgent
	{
		public static double totaltime=0;
		public static Score MyAttSum = new MyAttackDamage();
		public static Score OpAttSum = new OpAttackDamage();
		public static Score myminiHealth = new MyMinionHealth();
		public static Score opminiHealth = new OpMinionHealth();
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
		public override void InitializeAgent()
		{
		}
		public override void InitializeGame()
		{
		}
		public override void FinalizeAgent()
		{
		}
		public override void FinalizeGame()
		{
		}
		public override PlayerTask GetMove(POGame poGame)
		{
			var player = poGame.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			
			List<PlayerTask> options = poGame.CurrentPlayer.Options();
			PlayerTask best = options[0];



			if (options.Count == 1)
			{
				best = options[0];
			}

			//MctsAgent
			else if ((poGame.CurrentPlayer.HeroClass == SabberStoneCore.Enums.CardClass.WARRIOR ||
						poGame.CurrentPlayer.HeroClass == SabberStoneCore.Enums.CardClass.PALADIN) &&
						(poGame.CurrentPlayer.Opponent.HeroClass != SabberStoneCore.Enums.CardClass.MAGE &&
							poGame.CurrentPlayer.Opponent.HeroClass != SabberStoneCore.Enums.CardClass.ROGUE &&
							poGame.CurrentPlayer.Opponent.HeroClass != SabberStoneCore.Enums.CardClass.DRUID &&
							poGame.CurrentPlayer.Opponent.HeroClass != SabberStoneCore.Enums.CardClass.PRIEST &&
							poGame.CurrentPlayer.Opponent.HeroClass != SabberStoneCore.Enums.CardClass.HUNTER))
						 					
			{			
				if (poGame.Turn <= 2)
				{
					best = MCTS.PlayMCTS(poGame, 7.2);
				}
				if (poGame.Turn <= 6)
				{
					best = MCTS.PlayMCTS(poGame, 6);
				}
				else if (poGame.Turn <= 14)
				{
					best = MCTS.PlayMCTS(poGame, 4.2);
				}
				else
				{
					best = MCTS.PlayMCTS(poGame, 3.7);
				}

			}

			//GenticProgrammingAgent
			else 
			{


				TreeScore.tree_node = null;
				switch (poGame.CurrentPlayer.HeroClass)
				{
					case SabberStoneCore.Enums.CardClass.MAGE:
						TreeScore.tree_node = "feHhGCDcGHkBaCB";
						break;

					default:
						TreeScore.tree_node = "kaeHDimBcDCBjB";
						break;
				}

				List<PlayerTask> gpOptions = poGame.CurrentPlayer.Options();

				double candidateScore = Double.MaxValue;


				foreach (PlayerTask task in gpOptions)
				{
					if (task.PlayerTaskType != PlayerTaskType.CONCEDE && task.PlayerTaskType != PlayerTaskType.END_TURN)
					{
						try
						{
							Dictionary<PlayerTask, POGame> dic = poGame.Simulate(new List<PlayerTask> { task });
							POGame gameC = dic[task];//update game state
							if (gameC != null)
							{
								Controller my = gameC.CurrentPlayer;
								Controller op = gameC.CurrentPlayer.Opponent;

								MyAttSum.Controller = my;
								OpAttSum.Controller = my;
								myminiHealth.Controller = my;
								opminiHealth.Controller = my;
								//分配數值
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

								double score = Math.Round(TreeScore.Node_Evaluation(num_my_board, num_op_board, num_my_hand, num_op_hand,
						num_my_hero, num_op_hero, num_my_deck, num_op_deck, num_remaining_mana, num_my_attackdamage, num_op_attackdamage,
						num_my_minionHealth, num_op_minionHealth), 15, MidpointRounding.AwayFromZero);

								if (num_op_hero < 1)
								{
									score = Double.MinValue;

								}

								if (num_my_hero < 1)
								{
									score = Double.MaxValue;

								}

								if (score <= candidateScore)
								{
									candidateScore = score;

									best = task;
								}

							}

						}
						catch (Exception e)
						{
							continue;
						}

					}

				}
				if (best == null)
				{
					best = gpOptions[0];
				}

			}
			
			
			return best;

		}//getMove



	}


}
