namespace RailwayWars.ContestRunner
{
    public struct PlayerOffer : IPlayerCommand
    {
        public string PlayerId { get; }
        public Cell Cell { get; }
        public int Price { get; }

        public PlayerOffer(string playerId, Cell cell, int price)
        {
            PlayerId = playerId;
            Cell = cell;
            Price = price;
        }

        public PlayerOffer(string playerId, int x, int y, int price)
        {
            PlayerId = playerId;
            Cell = new Cell(x, y);
            Price = price;
        }
    }
}
