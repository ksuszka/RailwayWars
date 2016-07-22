using System;

namespace RailwayWars.ContestRunner
{
    public static class LocationExtensions
    {
        public static int HexManhattanDistance(this ILocation from, ILocation to)
        {
            //     4 4 4 4 4        -> 4 4 4 4 4
            //    4 3 3 3 3 4       -> 4 3 3 3 3 4
            //   4 3 2 2 2 3 4      -> 4 3 2 2 2 3 4
            //  4 3 2 1 1 2 3 4     -> 4 3 2 1 1 2 3 4
            // 4 3 2 1 0 1 2 3 4    -> 4 3 2 1 0 1 2 3 4
            //  4 3 2 1 1 2 3 4     ->   4 3 2 1 1 2 3 4
            //   4 3 2 2 2 3 4      ->     4 3 2 2 2 3 4
            //    4 3 3 3 3 4       ->       4 3 3 3 3 4
            //     4 4 4 4 4        ->         4 4 4 4 4
            return (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) + Math.Abs(from.X - to.X - from.Y + to.Y)) / 2;
        }
    }
}
