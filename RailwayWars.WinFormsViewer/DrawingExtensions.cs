using System.Drawing;

namespace RailwayWars.WinFormsViewer
{
    internal static class DrawingExtensions
    {
        public static PointF Offset(this PointF p, float dx, float dy) => PointF.Add(p, new SizeF(dx, dy));
        public static void DrawPoint(this Graphics g, Pen pen, PointF p) => g.DrawLine(pen, p.Offset(-0.1f, 0), p.Offset(0.1f, 0));
    }
}
