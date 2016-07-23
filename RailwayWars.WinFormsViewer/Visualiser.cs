using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using RailwayWars.Contracts;

namespace RailwayWars.WinFormsViewer
{
    internal class Visualiser
    {
        private readonly Pen _cityPen;
        private readonly Pen _trackPen;

        private static readonly Brush CityBackgroundBrush = Brushes.Black;
        private static readonly Brush BackgroundBrush = Brushes.Blue;
        private static readonly Brush[] FreeCellBrushes;

        private readonly int _cellWidth;
        private readonly int _cellHeight;

        public bool ShowTracksForeground { get; set; }

        static Visualiser()
        {
            FreeCellBrushes = Enumerable.Range(0, 200).Select(i => new SolidBrush(Color.FromArgb(0, 255 - i, 0))).ToArray();
        }

        public Visualiser(int cellWidth)
        {
            _cellWidth = cellWidth;
            _cellHeight = (int)Math.Round(cellWidth * Math.Sqrt(3) / 2);
            _cityPen = new Pen(Color.Gold, _cellWidth / 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            _trackPen = new Pen(Color.Black, _cellWidth / 5)
            {
                DashStyle = DashStyle.Custom,
                DashCap = DashCap.Flat,
                DashPattern = new float[] { 0.2f, 0.5f }
            };
        }

        class Transformation
        {
            readonly int _minPrice;
            readonly int _maxPrice;
            readonly Dictionary<string, Brush> _railwayBrushes = new Dictionary<string, Brush>();
            readonly Dictionary<string, Pen> _railwayPens = new Dictionary<string, Pen>();

            public int BoxWidth { get; set; }
            public int BoxHeight { get; set; }
            private int CellWidth { get; set; }
            private int CellHeight { get; set; }
            private int OffsetX { get; set; }
            private int OffsetY { get; set; }

            public Transformation(int cellWidth, int cellHeight, GameStateDTO gs)
            {
                CellWidth = cellWidth;
                CellHeight = cellHeight;

                var allCells = new HashSet<CellDTO>();
                allCells.UnionWith(gs.Cities);
                allCells.UnionWith(gs.FreeCells.Select(c => (CellDTO)c));
                allCells.UnionWith(gs.Railways.SelectMany(r => r.OwnedCells));

                Func<CellDTO, PointF> mapCenter = c => new PointF(c.X * cellWidth - c.Y * cellWidth / 2f, c.Y * cellHeight);
                var cells = allCells.Select(mapCenter);
                var minx = cells.Select(c => c.X).Min() - cellWidth;
                var maxx = cells.Select(c => c.X).Max() + cellWidth;
                var miny = cells.Select(c => c.Y).Min() - cellHeight;
                var maxy = cells.Select(c => c.Y).Max() + cellHeight;

                OffsetX = (int)(-minx);
                OffsetY = (int)(-miny);
                BoxWidth = (int)(maxx - minx);
                BoxHeight = (int)(maxy - miny);
                _minPrice = gs.MinPrice;
                _maxPrice = Math.Max(gs.MaxPrice, _minPrice + 1);

                {
                    var railways = gs.Railways.ToList();
                    for (int i = 0; i < railways.Count; i++)
                    {
                        var railway = railways[i];
                        _railwayBrushes[railway.Id] = new SolidBrush(ColorExtensions.FromHSV(30 + (i * 360.0 / railways.Count), 0.2, 1));
                        _railwayPens[railway.Id] = new Pen(ColorExtensions.FromHSV(30 + (i * 360.0 / railways.Count), 1, 0.8), cellWidth / 5) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    }
                }
            }

            public Brush RailwayBrush(string playerId) => _railwayBrushes[playerId];
            public Pen RailwayPen(string playerId) => _railwayPens[playerId];

            public Brush FreeCellBrush(FreeCellDTO cell)
            {
                var index = Visualiser.FreeCellBrushes.Length * (cell.Price - _minPrice) / (_maxPrice - _minPrice + 1);
                index = Math.Max(0, Math.Min(Visualiser.FreeCellBrushes.Length - 1, index));
                return Visualiser.FreeCellBrushes[index];
            }

            public PointF CellToCenter(int x, int y)
            {
                return new PointF(OffsetX + x * CellWidth - y * CellWidth / 2f, OffsetY + y * CellHeight);
            }

            public PointF CellToCenter(CellDTO c) => CellToCenter(c.X, c.Y);

            public PointF[] CellToHex(FreeCellDTO cell) => CellToHex((CellDTO)cell);

            public PointF[] CellToHex(CellDTO cell)
            {
                return new[]
                {
                    CellToCenter(cell.X, cell.Y).Offset(CellWidth/2 - CellWidth, -CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(0, -2*CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(CellWidth/2, -CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(CellWidth/2, CellHeight - 2*CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(0, CellHeight - CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(CellWidth/2 - CellWidth, CellHeight - 2*CellHeight/3),
                    CellToCenter(cell.X, cell.Y).Offset(CellWidth/2 - CellWidth , -CellHeight/3)
                };
            }
        }

        public Bitmap CreateBoardImage(GameStateDTO gs)
        {
            var t = new Transformation(_cellWidth, _cellHeight, gs);
            var bitmap = new Bitmap(t.BoxWidth, t.BoxHeight);

            using (var g = Graphics.FromImage(bitmap))
            {
                // background
                g.FillRectangle(BackgroundBrush, 0, 0, bitmap.Width, bitmap.Height);

                // free cell backgrounds
                gs.FreeCells.ToList()
                  .ForEach(c => g.FillPolygon(t.FreeCellBrush(c), t.CellToHex(c)));

                // railways backgrounds
                gs.Railways.ToList()
                  .ForEach(r =>
                  {
                      var brush = t.RailwayBrush(r.Id);
                      r.OwnedCells.ToList().ForEach(c => g.FillPolygon(brush, t.CellToHex(c)));
                  });

                // railways tracks backgrounds
                gs.Railways.ToList()
                  .ForEach(r =>
                  {
                      // first draw center points of each owned cell
                      var pen = t.RailwayPen(r.Id);
                      r.OwnedCells.ToList().ForEach(c =>
                      {
                          var p = t.CellToCenter(c);
                          g.DrawPoint(pen, p);
                      });

                      // next draw background tracks connecting two point of a railway (including cities)
                      var railwayPoints = new HashSet<CellDTO>(r.OwnedCells.Union(gs.Cities));
                      var tracks = railwayPoints.SelectMany(c =>
                        {
                            var p = t.CellToCenter(c);
                            return c.GetUniDirectionalNeighbours()
                             .Where(n => railwayPoints.Contains(n))
                                    .Select(n => Tuple.Create(p, t.CellToCenter(n)));
                        }).ToList();

                      tracks.ForEach(track => g.DrawLine(pen, track.Item1, track.Item2));

                      // railways tracks foregrounds
                      if (ShowTracksForeground)
                      {
                          tracks.ForEach(track => g.DrawLine(_trackPen, track.Item1, track.Item2));
                      }
                  });

                // cities backgrounds
                gs.Cities.ToList()
                  .ForEach(c => g.FillPolygon(CityBackgroundBrush, t.CellToHex(c)));

                // cities foreground
                gs.Cities.ToList()
                  .ForEach(c =>
                  {
                      var p = t.CellToCenter(c);
                      g.DrawPoint(_cityPen, p);
                  });
            }

            return bitmap;
        }
    }
}
