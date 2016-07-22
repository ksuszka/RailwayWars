using System;
using System.Collections.Generic;

namespace RailwayWars.ContestRunner
{
    public struct BoardDefinition
    {
        public IReadOnlyList<Cell> Cities { get; }
        public IReadOnlyList<FreeCell> FreeCells { get; }
        public TimeSpan TurnTime { get; set; }
        public int TurnProfit { get; set; }
        public int MaxInactivityTurns { get; set; }

        public BoardDefinition(IReadOnlyList<Cell> cities, IReadOnlyList<FreeCell> freeCells, TimeSpan turnTime, int turnProfit, int maxInactivityTurns)
        {
            TurnProfit = turnProfit;
            Cities = cities;
            FreeCells = freeCells;
            TurnTime = turnTime;
            MaxInactivityTurns = maxInactivityTurns;
        }

        public override string ToString()
        {
            return string.Format("[BoardDefinition: Cities={0}, FreeCells={1}, TurnTime={2}, TurnProfit={3}]", Cities.Count, FreeCells.Count, TurnTime.TotalMilliseconds, TurnProfit);
        }
    }
}
