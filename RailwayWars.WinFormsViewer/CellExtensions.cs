using RailwayWars.Contracts;

namespace RailwayWars.WinFormsViewer
{
    internal static class CellExtensions
    {
        public static CellDTO[] GetNeighbours(this CellDTO cell)
        {
            return new[]
            {
                new CellDTO() { X = cell.X + 1, Y = cell.Y},
                new CellDTO() { X = cell.X - 1, Y = cell.Y},
                new CellDTO() { X = cell.X, Y = cell.Y + 1},
                new CellDTO() { X = cell.X, Y = cell.Y - 1},
                new CellDTO() { X = cell.X + 1, Y = cell.Y + 1},
                new CellDTO() { X = cell.X - 1, Y = cell.Y - 1}
            };
        }

        public static CellDTO[] GetUniDirectionalNeighbours(this CellDTO cell)
        {
            return new[]
            {
                new CellDTO() { X = cell.X + 1, Y = cell.Y},
                new CellDTO() { X = cell.X, Y = cell.Y + 1},
                new CellDTO() { X = cell.X + 1, Y = cell.Y + 1}
            };
        }
    }
}
