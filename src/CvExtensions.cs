using OpenCvSharp;
using System;

namespace OpenCvSharp
{
    public static class CvExtensions
    {
        public static bool IsEmpty(this Size size) => size.Width == 0 && size.Height == 0;

        public static bool IsEmpty(this Point point) => point.X == 0 && point.Y == 0;

        public static Rect ToRect(this Size size) => new Rect(new Point(), size);

        public static Rect ToRect(this Mat mat) => new Rect(0, 0, mat.Width, mat.Height);

        public static Rect Shrink(this Rect rect, int size) => Shrink(rect, size, size);
        public static Rect Shrink(this Rect rect, int x, int y) =>
            Rect.FromLTRB(
                Math.Max(0, rect.Left + x),
                Math.Max(0, rect.Top + y),
                Math.Max(0, rect.Right - x),
                Math.Max(0, rect.Bottom - y)
            );

        public static Rect MinSize(this Rect rect, Size size) =>
            new Rect(rect.Left, rect.Top, Math.Max(rect.Width, size.Width), Math.Max(rect.Height, size.Height));

        public static bool TooCloseToBounds(this Rect rect, Rect bounds, int margin)
        {
            return rect.Left - margin < bounds.Left ||
                   rect.Right + margin > bounds.Right ||
                   rect.Top - margin < bounds.Top ||
                   rect.Bottom + margin > bounds.Bottom;
        }

        public static bool TooCloseToBounds(this Point point, Rect bounds, int margin)
        {
            return point.X - margin < bounds.Left ||
                   point.X + margin > bounds.Right ||
                   point.Y - margin < bounds.Top ||
                   point.Y + margin > bounds.Bottom;
        }

        public static double Magnitude(this Scalar scalar)
        {
            return Math.Sqrt(scalar.Val0 * scalar.Val0 + scalar.Val1 * scalar.Val1 + scalar.Val2 * scalar.Val2 + scalar.Val3 * scalar.Val3);
        }

        public static float Area(this Rect rect) => rect.Width * rect.Height;

        public static Rect ToRect(this string[] parts)
        {
            if (
                parts.Length == 4 &&
                int.TryParse(parts[0], out int l) &&
                int.TryParse(parts[1], out int t) &&
                int.TryParse(parts[2], out int r) &&
                int.TryParse(parts[3], out int b)
            )
                return Rect.FromLTRB(l, t, r, b);

            return Rect.Empty;
        }

        public static Point Center(this Mat mat)
        {
            return new Point(mat.Width / 2, mat.Height / 2);
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public static bool IsValidRoi(this Rect rect)
        {
            return rect.X > 0 && rect.Y > 0 && rect.Width > 0 && rect.Height > 0;
        }

        public static Size Scale(this Size size, double scale) => new Size(size.Width * scale, size.Height * scale);

        public static Rect Scale(this Rect input, double scale) => new Rect(input.TopLeft * scale, Scale(input.Size, scale));
    }


}