using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PageCompare
{
    public class Differ
    {
        public double Threshold { get; set; } = 5.0;

        public Mat StructuringElement { get; set; } = Mat.Ones(new Size(8, 8), MatType.CV_8U);

        public Size NearSize { get; set; } = new Size(40, 200);

        public double ThumbnailWidth { get; set; } = 150.0;

        public Size GridCellSize { get; set; } = new Size(20, 20);

        public Size MinThumbnailMarkerSize { get; set; } = new Size(20, 20);

        public Scalar GridColor { get; set; } = Scalar.Gray;

        public Size ImageSpacing { get; set; } = new Size(20, 20);

        public int ImageWidth { get; set; } = 600;

        public Comparer<Rect> RectComparer { get; set; } =
            Comparer<Rect>.Create((a, b) => a.Top.CompareTo(b.Top));

        static void DrawGrid(Mat input, Size gridSize, Scalar scalar, int thickness = 1)
        {
            var (width, height) = (input.Width, input.Height);
            for (int x = 0; x < width; x += gridSize.Width)
                Cv2.Line(input, x, 0, x, height, scalar, thickness);

            for (int y = 0; y < height; y += gridSize.Height)
                Cv2.Line(input, 0, y, width, y, scalar, thickness);

            Cv2.CopyMakeBorder(input, input, thickness, thickness, thickness, thickness, BorderTypes.Constant, scalar);
        }

        static Mat Resize(Mat input, Size targetSize, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            var output = new Mat(targetSize, input.Type());
            Cv2.Resize(input, output, targetSize, interpolation: interpolation);
            return output;
        }

        public IEnumerable<Mat> ComputeDiff(Mat src1, Mat src2)
        {
            using var thresh = new Mat();
            using (var diff = new Mat())
            {
                Cv2.Absdiff(src1, src2, diff);
                Cv2.Threshold(diff, thresh, Threshold, 255.0, ThresholdTypes.Binary);
            }

            Point[][] contours;

            using (var dil = new Mat())
            {
                Cv2.Dilate(thresh, dil, StructuringElement, borderType: BorderTypes.Constant, iterations: 1);
                using var precont = new Mat();
                Cv2.CvtColor(dil, precont, ColorConversionCodes.RGB2GRAY);
                contours = Cv2.FindContoursAsArray(precont, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            }

            var rects = Array.ConvertAll(contours, Cv2.BoundingRect);

            bool Near(Rect a, Rect b, Size size) =>
                Rect.Inflate(a, size.Width, size.Height).IntersectsWith(b);

            var combined = rects.Aggregate(new List<Rect>(), (acc, r) =>
            {
                bool anyIntersect = false;
                for (int i = 0; i < acc.Count; i++)
                {
                    if (Near(acc[i], r, NearSize))
                    {
                        acc[i] = Rect.Union(r, acc[i]);
                        anyIntersect = true;
                    }
                }

                if (!anyIntersect)
                    acc.Add(r);

                return acc;
            });


            combined.Sort(RectComparer);

            var thumnbailSize = new Coords(src2) { ConstrainWidth = ThumbnailWidth };
            using var thumbnail = Resize(src2, thumnbailSize.RelativeSize);

            Cv2.CopyMakeBorder(thumbnail, thumbnail, 2, 2, 2, 2, BorderTypes.Constant, Scalar.Red);

            void DrawImage(Mat input, Coords coords, Mat output)
            {
                using var small = new Mat(output, new Rect(coords.RelativeOrigin, input.Size()));
                Cv2.CopyTo(input, small);
            }

            foreach (Rect rect in combined)
            {
                var c_diff = new Coords(rect.Size) { ConstrainWidth = ImageWidth };

                var c_canvas = new Coords(new Size(ImageWidth + ImageSpacing.Width + thumbnail.Width, Math.Max(c_diff.RelativeHeight * 2 + ImageSpacing.Height * 2, thumbnail.Height + ImageSpacing.Height)));
                var c_thumbnail = new Coords(src1, c_canvas) { ConstrainWidth = thumbnail.Width };

                var canvas = new Mat(c_canvas.Size, thumbnail.Type(), Scalar.White);

                DrawImage(thumbnail, c_thumbnail, canvas);

                var thumb_marker_rect = c_thumbnail.Project(rect).MinSize(MinThumbnailMarkerSize);
                Cv2.Rectangle(canvas, thumb_marker_rect, Scalar.Red, -1);


                var c_before = new Coords(c_diff, c_canvas)
                {
                    Origin = new Point(c_canvas.Size.Width - ImageWidth, 0)
                };

                var c_after = new Coords(c_diff, c_canvas)
                {
                    Origin = c_before.Origin + new Point(0, c_before.RelativeHeight + ImageSpacing.Height)
                };

                using (
                    Mat diff1 = Resize(src1.SubMat(rect), c_diff.RelativeSize),
                        diff2 = Resize(src2.SubMat(rect), c_diff.RelativeSize)
                )
                {
                    DrawImage(diff1, c_before, canvas);
                    DrawImage(diff2, c_after, canvas);
                }


                Mat diff1r = canvas.SubMat(c_before.RelativeBounds);
                Mat diff2r = canvas.SubMat(c_after.RelativeBounds);

                using (var absDiff = new Mat())
                {
                    Cv2.Absdiff(diff1r, diff2r, absDiff);

                    using var threshold = new Mat();
                    Cv2.Threshold(absDiff, threshold, 1.0, 255.0, ThresholdTypes.Binary);

                    Cv2.AddWeighted(diff1r, 0.9, threshold, 0.2, 0, diff1r);
                    Cv2.AddWeighted(diff2r, 0.9, threshold, 0.2, 0, diff2r);
                }

                DrawGrid(diff1r, GridCellSize, GridColor, 1);
                DrawGrid(diff2r, GridCellSize, GridColor, 1);

                yield return canvas;
            }
        }

    }
}
