using OpenCvSharp;
using OpenCvSharp.XImgProc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PageCompare
{

    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("page-compare before.png after.png [filename]");
                Console.WriteLine("If no filename is specified, the results are displayed in new windows.");
                return -1;
            }

            using var src1 = Mat.FromStream(File.OpenRead(args[0]), ImreadModes.AnyColor);
            using var src2 = Mat.FromStream(File.OpenRead(args[1]), ImreadModes.AnyColor);

            var differ = new Differ();
            var diffs = differ.ComputeDiff(src1, src2).Select((d, i) => (d, i + 1));
            var hasFilename = args.Length >= 3;
            var fileName = args.ElementAtOrDefault(2);

            foreach (var (diff, index) in diffs)
            {
                using (diff)
                {
                    if (hasFilename)
                        Cv2.ImWrite($"{fileName}{index}.png", diff);
                    else
                        Cv2.ImShow($"Diff-{index}", diff);
                }
            }

            if (!hasFilename)
                Cv2.WaitKey();

            return 0;
        }


    }
}
