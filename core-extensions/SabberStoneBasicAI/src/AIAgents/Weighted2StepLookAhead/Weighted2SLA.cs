using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using System.Linq;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.AIAgents.CSC570
{
	class BetterScore : Score.Score
	{
		public override int Rate()
		{
			GretiveScore totalScore = Weighted2SLA.GetScore();

			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			int score = 0;

			score += 3 * HeroHp;
			score -= 4 * OpHeroHp;

			score +=  4 * BoardZone.Count;
			score -=  6 * OpBoardZone.Count;

			foreach (var boardZoneEntry in BoardZone)
			{
				score += 5 * boardZoneEntry.Health;
				score += 4 * boardZoneEntry.AttackDamage;
			}

			foreach (var boardZoneEntry in OpBoardZone)
			{
				score -= 9 * boardZoneEntry.Health;
				score -= 7 * boardZoneEntry.AttackDamage;
			}

			return totalScore.Rate() + score;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}


	class Weighted2SLA : AbstractAgent
	{
		private SabberStoneCore.Model.Entities.Controller _player;
		private bool _initialized = false;

		public static GretiveScore totalScore;

		public static GretiveScore GetScore()
		{
			return totalScore;
		}
		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(POGame game)
		{
			_player = game.CurrentPlayer;
			if (!_initialized) InitByHero(_player.HeroClass);

			var player = game.CurrentPlayer;
			var validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
			var optcount = validOpts.Count();

			var returnValue = validOpts.Any() ?
				validOpts.Select(x => score(x, player.PlayerId, (optcount >= 5) ? ((optcount >= 25) ? 1 : 2) : 3)).OrderBy(x => x.Value).Last().Key :
				player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			return returnValue;

			KeyValuePair<PlayerTask, int> score(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
				int max_score = int.MinValue;
				if (max_depth > 0 && state.Value.CurrentPlayer.PlayerId == player_id)
				{
					var subactions = state.Value.Simulate(state.Value.CurrentPlayer.Options()).Where(x => x.Value != null);

					foreach (var subaction in subactions)
						max_score = Math.Max(max_score, score(subaction, player_id, max_depth - 1).Value);


				}
				max_score = Math.Max(max_score, Score(state.Value, player_id));
				return new KeyValuePair<PlayerTask, int>(state.Key, max_score);
			}
		}

		private void InitByHero(CardClass heroClass, Profile profile = Profile.DEFAULT_BY_HERO)
		{
			totalScore = GretiveDispatcher.Score(heroClass, profile);
			_initialized = true;
		}

		private int Score(POGame state, int playerId)
		{
			totalScore.Controller = state.CurrentPlayer.PlayerId == _player.PlayerId ? state.CurrentPlayer : state.CurrentOpponent;

			return new BetterScore { Controller = totalScore.Controller }.Rate();
		}

		public override void InitializeAgent()
		{
		}

		public override void InitializeGame()
		{
		}
	}
}
