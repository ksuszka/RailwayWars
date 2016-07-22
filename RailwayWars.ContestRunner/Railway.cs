using System.Collections.Generic;

namespace RailwayWars.ContestRunner
{
    public class Railway
    {
        public string Id { get; set; }
        public int Money { get; set; }
        public int Score { get; set; }
        public ISet<Cell> OwnedCells { get; set; }
        public int ConnectedCitiesCount { get; set; }
    }
}
