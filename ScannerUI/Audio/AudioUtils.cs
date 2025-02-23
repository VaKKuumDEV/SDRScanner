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

        public static float Correlation(this IEnumerable<float> nums1, IEnumerable<float> nums2)
        {
            float avg1 = nums1.Average();
            float avg2 = nums2.Average();

            float sum1 = nums1.Zip(nums2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            float sumSqr1 = nums1.Sum(x => (float)Math.Pow(x - avg1, 2.0f));
            float sumSqr2 = nums2.Sum(y => (float)Math.Pow(y - avg2, 2.0f));

            float correl = sum1 / (float)Math.Sqrt(sumSqr1 * sumSqr2);
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

        public unsafe static void SimpleAverage(float* values, float* averagedValues, int length, int k)
        {
            Queue<float> valuesQueue = [];
            for (int i = 0; i < length; i++)
            {
                valuesQueue.Enqueue(values[i]);

                float average = valuesQueue.Sum() / k;
                averagedValues[i] = average;

                if (valuesQueue.Count == k) valuesQueue.Dequeue();
            }
        }
    }
}
