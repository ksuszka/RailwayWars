using System.Collections.Generic;

namespace RailwayWars.Contracts
{
    public class GameStateDTO
    {
        public int Turn { get; set; }
        public int TurnTimeMilliseconds { get; set; }
        public ISet<CellDTO> Cities { get; set; }
        public IEnumerable<FreeCellDTO> FreeCells { get; set; }
        public IEnumerable<RailwayDTO> Railways { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int TurnProfit { get; set; }
    }
}
