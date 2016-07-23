using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayWars.ContestRunner.Rating
{
    class OrderedRatingStrategy<TPlayerId> : IRatingStrategy<TPlayerId>
    {
        public int InitialRating => 0;

        public IList<PlayerRating<TPlayerId>> Calculate(IEnumerable<PlayerRatingScore<TPlayerId>> players)
        {
            var playerMap = players.ToDictionary(p => p.Id, p => p.Rating);
            // Assign tournament points based on order of scored points
            var sortedScores = players.GroupBy(k => k.Score, k => k.Id).OrderByDescending(g => g.Key);
            var points = players.Count();
            foreach (var score in sortedScores)
            {
                score.ToList().ForEach(playerId => playerMap[playerId] += points);
                points -= score.Count();
            }
            return playerMap.Select(kv => new PlayerRating<TPlayerId>(kv.Key, kv.Value)).ToList();
        }
    }
}
