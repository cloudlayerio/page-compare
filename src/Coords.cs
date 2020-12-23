using OpenCvSharp;
using System;

namespace PageCompare
{
    public class Coords
    {
        public Coords Parent { get; }

        public Rect Bounds { get; set; }

        public double Scale { get; set; } = 1.0;

        public Coords(Rect bounds, Coords parent = null)
        {
            Bounds = bounds;
            Parent = parent;
        }

        public Coords(Coords coords, Coords parent = null) : this(coords.RelativeBounds, coords.Parent ?? parent)
        {

        }

        public Coords(Size size, Coords parent = null) :
            this(new Rect(Zero, size), parent)
        { }

        public Coords(Mat mat, Coords parent = null) :
            this(mat.Size(), parent)
        { }


        public double RelativeScale =>
            Parent == null ? Scale : Scale * Parent.RelativeScale;

        public Point Origin
        {
            get => Bounds.TopLeft;
            set => Bounds = new Rect(value, Bounds.Size);
        }

        public static Point Zero = new Point(0, 0);

        public Point RelativeOrigin =>
            (Parent?.RelativeOrigin ?? Zero) + (Origin * RelativeScale);
        public Rect RelativeBounds => new Rect(RelativeOrigin, RelativeSize);
        public Size Size
        {
            get => Bounds.Size;
            set => Bounds = new Rect(Origin, value);
        }

        public Size RelativeSize => Size.Scale(RelativeScale);

        public double ConstrainWidth
        {
            set => Scale = value / Bounds.Width;
        }

        public double ConstrainHeight
        {
            set => Scale = value / Bounds.Height;
        }

        public double RelativeWidth => Bounds.Width * RelativeScale;

        public double RelativeHeight => Bounds.Height * RelativeScale;

        public Coords At(Rect rect) => new Coords(rect, this);
        public Coords At(Point point, Size size) => At(new Rect(point, size));

        public Point Project(Point point) => point * RelativeScale + RelativeOrigin;
        public Rect Project(Rect rect) => new Rect(Project(rect.TopLeft), rect.Size.Scale(RelativeScale));

        public override string ToString() => $"{RelativeBounds} ({Math.Round(Scale, 2)})";
    }
}
