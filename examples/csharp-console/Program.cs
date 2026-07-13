using System;
using System.Diagnostics;
using System.IO;

namespace LwPPOCRNet35
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: LwPPOCRNet35.exe det.onnx rec.onnx dict.txt image [loops]");
                return 2;
            }

            int loops = 5;
            if (args.Length >= 5 && !int.TryParse(args[4], out loops)) loops = 5;
            loops = Math.Max(1, loops);

            try
            {
                byte[] image = File.ReadAllBytes(Path.GetFullPath(args[3]));
                Stopwatch initWatch = Stopwatch.StartNew();
                using (PPOCRNative ocr = new PPOCRNative(
                    Path.GetFullPath(args[0]),
                    Path.GetFullPath(args[1]),
                    Path.GetFullPath(args[2]),
                    0))
                {
                    initWatch.Stop();
                    Console.WriteLine("Initialization: {0:F1} ms", initWatch.Elapsed.TotalMilliseconds);

                    ocr.Recognize(image);
                    double best = double.MaxValue;
                    double total = 0;
                    string json = null;
                    for (int i = 0; i < loops; i++)
                    {
                        Stopwatch watch = Stopwatch.StartNew();
                        json = ocr.Recognize(image);
                        watch.Stop();
                        double elapsed = watch.Elapsed.TotalMilliseconds;
                        best = Math.Min(best, elapsed);
                        total += elapsed;
                        Console.WriteLine("Run {0}: {1:F1} ms", i + 1, elapsed);
                    }

                    Console.WriteLine("Best: {0:F1} ms, Average: {1:F1} ms", best, total / loops);
                    Console.WriteLine(json);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}

