using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score.mudgal_kumar_1;

namespace SabberStoneBasicAI.AIAgents.mudgal_kumar_1
{
    /***
    This is Implemented by Tom Heimbrodt and winner of the 2019 Hearthstone AI Competition's Premade Deck Playing Track
    Modification:
    
    Tom's agent uses a Greedy BFS lookahead.
    This uses a greedy DFS lookahead.
    "State|Score" => child States => sort by score at the moment(no lookahead) => Choose best PlayerTaskInstance
    => Expand Child States => Repeat until max_depth 

    ***/
    class GreedyLookaheadAgent: AbstractAgent{
       	public override void FinalizeAgent(){}
        public override void FinalizeGame(){}
        public override void InitializeAgent(){}
        public override void InitializeGame(){}
        KeyValuePair<PlayerTask, int> Simulate(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
                // Essesntially Tom's agent but does a DFS search instead of a BFS search
				int max_score = int.MinValue;
				if (max_depth > 0 && state.Value.CurrentPlayer.PlayerId == player_id)
				{
					var subactions = state.Value.Simulate(state.Value.CurrentPlayer.Options()).Where(x => x.Value != null);
                    // var scores = subactions.Select(x=>Score(x.Value,player_id));
                    // var subaction = subactions.OrderBy(x=>Simulate(x,player_id,max_depth-1).Value).Last();
                    var subaction = subactions.OrderBy(x=>Score(x.Value,player_id)).Last();
					max_score = Math.Max(max_score,Simulate(subaction,player_id,max_depth-1).Value);


				}
				max_score = Math.Max(max_score, Score(state.Value, player_id));
				return new KeyValuePair<PlayerTask, int>(state.Key, max_score);
			}

        private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new WeightedScore { Controller = p }.Rate();
		}
        

        public override PlayerTask GetMove(POGame game){
            var player = game.CurrentPlayer;

            // Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new WeightedScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

            //Lookhead n steps and choose the best scoring <PlayerTask> and each depth -> branch ->score->until depth
            //(DFS search)

            var validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
            var voptcount = validOpts.Count();

           
            if(validOpts.Any()){
                var depth = voptcount>5 ? (voptcount>25 ? 1: 2) : 3;
                var scored = validOpts.Select(x=>Simulate(x,player.PlayerId,depth));
                // Array.ForEach(scored.Select(x=>x.Item1).ToArray(),Console.Write);
                // Console.Write($"\r{scored.Count()}  ");
                return scored.OrderBy(x=>x.Value).Last().Key;
              
            }
            else{
                return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
            }

            


        }
    }

}
