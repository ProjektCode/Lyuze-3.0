using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lyuze.Core.Shared.Images.Primitives {

    public static class GraphicsUtils {

        public static void FillRoundedRect(Graphics g, Brush brush, RectangleF rect, float radius) {
            using var path = RoundedRectPath(rect, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRect(Graphics g, Pen pen, RectangleF rect, float radius) {
            using var path = RoundedRectPath(rect, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRectPath(RectangleF rect, float radius) {
            float r = Math.Max(1f, radius);
            float d = r * 2f;

            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
