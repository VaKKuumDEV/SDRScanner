using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Audio
{
    public static class AudioUtils
    {
        public static FilePickerFileType WavFiles { get; } = new("Wav Files")
        {
            Patterns = ["*.wav"],
            MimeTypes = ["audio/x-wav"]
        };

        public struct Point(double x, double y)
        {
            public enum Position
            {
                None,
                Top,
                Low,
            }

            public double X { get; } = x;
            public double Y { get; } = y;
            public Position Pos { get; set; } = Position.None;

            public override readonly string ToString()
            {
                return $"{X}:{Y}:{Pos}";
            }
        };

        public static double Correlation(this IEnumerable<double> nums1, IEnumerable<double> nums2)
        {
            double avg1 = nums1.Average();
            double avg2 = nums2.Average();

            double sum1 = nums1.Zip(nums2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            double sumSqr1 = nums1.Sum(x => (double)Math.Pow(x - avg1, 2.0f));
            double sumSqr2 = nums2.Sum(y => (double)Math.Pow(y - avg2, 2.0f));

            double correl = sum1 / (double)Math.Sqrt(sumSqr1 * sumSqr2);
            return correl;
        }

        public unsafe static void CumulativeSum(double* sequence, double* cumulativeSum, int length)
        {
            double sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += sequence[i];
                cumulativeSum[i] = sum;
            }
        }

        public unsafe static void IntegratedSpectrum(double* cumulativeSum, int length)
        {
            for (int i = 0; i < length; i++)
            {
                cumulativeSum[i] /= cumulativeSum[length - 1];
            }
        }

        public unsafe static void SimpleAverage(double* values, double* averagedValues, int length, int k)
        {
            Queue<double> valuesQueue = [];
            for (int i = 0; i < length; i++)
            {
                valuesQueue.Enqueue(values[i]);

                double average = valuesQueue.Sum() / valuesQueue.Count;
                averagedValues[i] = average;

                if (valuesQueue.Count == k) valuesQueue.Dequeue();
            }
        }
    }
}
