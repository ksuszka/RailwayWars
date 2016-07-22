namespace RailwayWars.ContestRunner
{
    public class FreeCell : ILocation
    {
        public int X { get; }
        public int Y { get; }
        public int Price { get; }

        public FreeCell(int x, int y, int price)
        {
            X = x;
            Y = y;
            Price = price;
        }

        public Cell Cell => new Cell(X, Y);
    }
}
