using System.Collections.Generic;

namespace RailwayWars.Contracts
{
    public struct RailwayDTO
    {
        public string Id { get; set; }
        public int Money { get; set; }
        public int Score { get; set; }
        public ISet<CellDTO> OwnedCells { get; set; }
        public int ConnectedCitiesCount { get; set; }
    }
}
