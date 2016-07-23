using System;
using System.Collections.Generic;

namespace RailwayWars.ContestRunner.Rating
{
    public struct PlayerRating<TPlayerId>
    {
        public TPlayerId Id { get; }
        public int Rating { get; }
        public PlayerRating(TPlayerId id, int rating) { Id = id; Rating = rating; }
    }

    public struct PlayerRatingScore<TPlayerId>
    {
        public TPlayerId Id { get; }
        public int Rating { get; }
        public int Score { get; }
        public PlayerRatingScore(TPlayerId id, int rating, int score) { Id = id; Rating = rating; Score = score; }
    }

    interface IRatingStrategy<TPlayerId>
    {
        int InitialRating { get; }
        IList<PlayerRating<TPlayerId>> Calculate(IEnumerable<PlayerRatingScore<TPlayerId>> players);
    }
}
