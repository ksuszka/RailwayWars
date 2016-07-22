namespace RailwayWars.Contracts
{
    public struct FreeCellDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Price { get; set; }

        public static explicit operator CellDTO(FreeCellDTO f)
        {
            return new CellDTO() { X = f.X, Y = f.Y };
        }
    }
}
