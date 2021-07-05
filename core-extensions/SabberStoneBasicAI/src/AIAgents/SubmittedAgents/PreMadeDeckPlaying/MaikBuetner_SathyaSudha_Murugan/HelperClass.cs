using System;
using System.Collections.Generic;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
//using System.Linq;
//using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.AIAgents.MaBuSaBu
{
	//class ScoreTest : Score.Score
	//{
	//	readonly int[] scalars = new int[]
	//	{
	//		43,
	//		-67,
	//		82,
	//		-39,
	//		108,
	//		121,
	//		-177,
	//		-169
	//	};

	//	public override int Rate()
	//	{
	//		if (HeroHp < 1)
	//			return int.MinValue;
	//		if (OpHeroHp < 1)
	//			return int.MaxValue;

	//		int retval = 0;

	//		retval += scalars[0] * HeroHp;
	//		retval += scalars[1] * OpHeroHp;
	//		retval += scalars[2] * BoardZone.Count;
	//		retval += scalars[3] * OpBoardZone.Count;

	//		foreach(var mob in BoardZone)
	//		{
	//			retval += scalars[4] * mob.Health;
	//			retval += scalars[5] * mob.AttackDamage;
	//		}
	//		foreach(var mob in OpBoardZone)
	//		{
	//			retval += scalars[6] * mob.Health;
	//			retval += scalars[7] * mob.AttackDamage;
	//		}
	//		return retval;
	//	}

	//	public override Func<List<IPlayable>, List<int>> MulliganRule()
	//	{
	//		return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
	//	}
	//}

	public enum EnumScores
	{
		HeroHealth,
		EnemyHeroHealth,
		HeroArmor,
		EnemyHeroArmor,

		MobHealth,
		MobAttack,
		EnemyMobHealth,
		EnemyMobAttack,
		MobCount,
		EnemyMobCount,

		PotentialDanger
	}

	class SimGame
	{
		public int step;
		public int depth;
		public PlayerTask[] tasks;
		public POGame game;
		public bool hasChildren => depth > 0;

		public SimGame[] Next()
		{
			if (depth <= 0)
				return new SimGame[0];
			var options = game.CurrentPlayer.Options();
			var retval = new SimGame[options.Count];

			var sims = game.Simulate(options);
			int i = 0;
			foreach (var sim in sims)
			{
				var newTasks = new PlayerTask[tasks.Length + 1];
				Array.Copy(tasks, newTasks, tasks.Length);
				newTasks[tasks.Length] = sim.Key;

				retval[i] = new SimGame(sim.Value, sim.Key.PlayerTaskType == PlayerTaskType.END_TURN ? 0 : depth - 1, newTasks, step + 1);
				i++;
			}

			return retval;
		}

		public override string ToString()
		{
			return tasks[0].ToString();
		}

		/// <summary>
		/// Gets the value of the current board / gane
		/// </summary>
		/// <param name="poGame"></param>
		/// <returns></returns>
		public int GameValue(int playerID, Dictionary<EnumScores, int> scoreDict)
		{
			var player = game.CurrentPlayer.PlayerId == playerID ? game.CurrentPlayer : game.CurrentOpponent;
			var opponent = player.Opponent;

			int potentialDanger = 1;

			// go for losing or winning strategies first
			if (player.Hero.Health < 1)
				return int.MinValue;
			if (opponent.Hero.Health < 1)
				return int.MaxValue;

			int retval = 0;
			// Evaluate Heroes
			retval += player.Hero.Health * scoreDict[EnumScores.HeroHealth];
			retval += opponent.Hero.Health * scoreDict[EnumScores.EnemyHeroHealth];
			potentialDanger -= player.Hero.Health;
			potentialDanger += opponent.Hero.AttackDamage;
			retval += player.Hero.Armor * scoreDict[EnumScores.HeroArmor];
			retval += opponent.Hero.Armor * scoreDict[EnumScores.EnemyHeroArmor];
			potentialDanger -= player.Hero.Armor;

			// Evaluate Amount of Mobs
			retval += player.BoardZone.Count * scoreDict[EnumScores.MobCount];
			retval += opponent.BoardZone.Count * scoreDict[EnumScores.EnemyMobCount];

			// Evaluate Mob Health and Damage
			foreach (var mob in player.BoardZone)
			{
				retval += mob.Health * scoreDict[EnumScores.MobHealth];
				retval += mob.AttackDamage * scoreDict[EnumScores.MobAttack];

			}
			foreach (var mob in opponent.BoardZone)
			{
				retval += mob.Health * scoreDict[EnumScores.EnemyMobHealth];
				retval += mob.AttackDamage * scoreDict[EnumScores.EnemyMobAttack];
				potentialDanger += mob.AttackDamage;
			}

			// Evaluate Danger State for Hero
			if (potentialDanger > 0)
				retval += 10 * scoreDict[EnumScores.PotentialDanger];
			else if (potentialDanger > -6)
				retval += (6 + potentialDanger) * scoreDict[EnumScores.PotentialDanger];

			return retval;
		}

		//public int AlternativeGameValue(int playerID) => new ScoreTest { Controller = playerID == game.CurrentPlayer.PlayerId ? game.CurrentPlayer : game.CurrentOpponent }.Rate();

		public SimGame(POGame _game, int _depth, PlayerTask[] _tasks, int _step)
		{
			game = _game;
			depth = _depth;
			tasks = _tasks;
			step = _step;
		}
		public SimGame(POGame _game, int _depth)
		{
			tasks = new PlayerTask[0];
			depth = _depth;
			step = 0;
			game = _game;
		}
	}
}
