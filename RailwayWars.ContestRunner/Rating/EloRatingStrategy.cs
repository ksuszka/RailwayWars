using System;
using System.Collections.Generic;
using System.Linq;

namespace RailwayWars.ContestRunner.Rating
{
    class EloRatingStrategy<TPlayerId> : IRatingStrategy<TPlayerId>
    {
        public int InitialRating => 1000;

        public IList<PlayerRating<TPlayerId>> Calculate(IEnumerable<PlayerRatingScore<TPlayerId>> players)
        {
            // Based on https://github.com/FigBug/Multiplayer-ELO/blob/master/csharp/elo.cs

            var newRatings = players.ToDictionary(p => p.Id, p => p.Rating);

            // Calculate rating change for combination of each player with each other player.
            var prs = players.ToList();
            var k = 32.0 / (prs.Count - 1);
            for (int i = 0; i < prs.Count; i++)
            {
                var player = prs[i];
                for (int j = i + 1; j < prs.Count; j++)
                {
                    var opponent = prs[j];

                    var s = (player.Score > opponent.Score) ? 1.0 : (player.Score < opponent.Score) ? 0.0 : 0.5;
                    var ea = 1 / (1.0 + Math.Pow(10.0, (opponent.Rating - player.Rating) / 400.0));
                    var delta = (int)Math.Round(k * (s - ea));
                    newRatings[player.Id] += delta;
                    newRatings[opponent.Id] -= delta;
                }
            }

            return newRatings.Select(kv => new PlayerRating<TPlayerId>(kv.Key, kv.Value)).ToList();
        }
    }
}
