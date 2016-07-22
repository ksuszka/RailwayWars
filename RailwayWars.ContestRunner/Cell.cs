namespace RailwayWars.ContestRunner
{
    public struct Cell : ILocation
    {
        public int X { get; }
        public int Y { get; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Cell(ILocation location)
        {
            X = location.X;
            Y = location.Y;
        }

        public Cell[] GetNeighbours()
        {
            return new[]
            {
                new Cell(X + 1, Y),
                new Cell(X - 1, Y),
                new Cell(X, Y + 1),
                new Cell(X, Y - 1),
                new Cell(X + 1, Y + 1),
                new Cell(X - 1, Y - 1)
            };
        }

        public override string ToString()
        {
            return string.Format("[Cell: X={0}, Y={1}]", X, Y);
        }
    }
}
