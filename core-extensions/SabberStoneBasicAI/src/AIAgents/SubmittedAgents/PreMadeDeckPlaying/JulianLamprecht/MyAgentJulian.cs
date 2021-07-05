using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using System.Linq;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using System.Timers;
using SabberStoneCore.Model;


namespace SabberStoneBasicAI.AIAgents.minimax
{
	class MiniMaxNode
	{
		public int id;
		public int parentNodeId;
		public List<int> childStates;
		public PlayerTask chosenActionToGetHere;
		public POGame gamestate;
		public bool isMaximizer;
		public int depth;
		public IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts;
		public int score;
	}

	class MyAgentJulian : AbstractAgent
	{
		static int MAX_DEPTH = 7;
		public Timer timer;
		public int playerId;
		public bool timeIsUp;
		public void whenTimeUp(object source, ElapsedEventArgs e) => timeIsUp = true;

		public override void InitializeAgent()
		{
		}

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override void InitializeGame()
		{
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			timeIsUp = false;
			timer = new Timer(28000);
			timer.Elapsed += whenTimeUp;
			timer.Enabled = true;

			Controller player = poGame.CurrentPlayer;
			playerId = player.PlayerId;

			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = poGame.Simulate(player.Options()).Where(x => x.Value != null);
			int countOptions = validOpts.Count();

			//AppendChildNodes(rootNode);

			int bestScore = Int32.MinValue;
			PlayerTask chosenTask = null;

			foreach (KeyValuePair<PlayerTask, POGame> option in validOpts)
			{
				int currentScore = MiniMax(0, option.Value, true, Int32.MinValue, Int32.MaxValue);
				//Console.WriteLine("Current Score: " + currentScore);
				if (currentScore > bestScore)
				{
					bestScore = currentScore;
					chosenTask = option.Key;
				}
			}
			//Console.WriteLine("Best Score: " + bestScore);

			if (chosenTask == null)
			{
				chosenTask = player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
			}

			//Console.WriteLine("Zugzeit " + stopWatch.Elapsed);
			//Console.WriteLine("Best Task: " + chosenTask);
			return chosenTask;
		}

		/*
		public void AppendChildNodes(MiniMaxNode parentNode)
		{
			Controller player;

			if (parentNode.depth == MAX_DEPTH) return;

			foreach (KeyValuePair<PlayerTask, POGame> option in parentNode.validOpts)
			{
				MiniMaxNode childNode = new MiniMaxNode();
				childNode.id = currentTree.getNextId();
				childNode.chosenActionToGetHere = option.Key;
				childNode.gamestate = option.Value;
				childNode.depth = parentNode.depth + 1;
				childNode.parentNodeId = parentNode.id;
				childNode.childStates = new List<int>();
				childNode.isMaximizer = !parentNode.isMaximizer;
				player = childNode.gamestate.CurrentPlayer;
				childNode.validOpts = childNode.gamestate.Simulate(player.Options()).Where(x => x.Value != null);
				parentNode.childStates.Add(childNode.id);
				currentTree.treeElements.Add(childNode.id, childNode);
				childNode.score = MiniMax(0, childNode.gamestate, childNode.isMaximizer, Int32.MinValue, Int32.MaxValue);
			}
		}
		*/

		//doesn't work lol, never goes beyond depth = 1, so its basically just a greedybot, fug
		public int MiniMax(int depth, POGame gamestate, bool isMaximizer, int alpha, int beta)
		{
			Controller player = gamestate.CurrentPlayer;

			if (!isMaximizer)
			{
				player = gamestate.CurrentPlayer;
			}
			else
			{
				player = gamestate.CurrentOpponent;
			}

			if (timeIsUp)
			{
				return Score(gamestate, playerId);
			}

			IEnumerable<KeyValuePair<PlayerTask, POGame>> childStates = gamestate.Simulate(player.Options()).Where(x => x.Value != null);

			if (depth == MAX_DEPTH || !childStates.Any())
			{
				return Score(gamestate, playerId);
			}

			if (isMaximizer)
			{
				int best = Int32.MinValue;

				foreach (KeyValuePair<PlayerTask, POGame> option in childStates)
				{
					int result = MiniMax(depth + 1, option.Value, !isMaximizer, alpha, beta);
						best = Math.Max(best, result);
						alpha = Math.Max(alpha, best);

						if (beta <= alpha) break;
				}
				return best;
			}

			else
			{
				int best = Int32.MaxValue;

				foreach (KeyValuePair<PlayerTask, POGame> option in childStates)
				{
					int result = MiniMax(depth + 1, option.Value, !isMaximizer, alpha, beta);
					best = Math.Min(best, result);
					beta = Math.Min(beta, best);

					if (beta <= alpha) break;
				}
				return best;
			}
		}

		//Scoring from Greedy Agent
		private int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			switch (state.CurrentPlayer.HeroClass)
			{
				case CardClass.WARRIOR: return new AggroScore { Controller = p }.Rate();
				case CardClass.MAGE: return new ControlScore { Controller = p }.Rate();
				default: return new RampScore { Controller = p }.Rate();
			}
		}
	}
}
