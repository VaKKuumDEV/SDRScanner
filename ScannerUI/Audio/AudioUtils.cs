using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Audio
{
    public static class AudioUtils
    {
        public const double SAMPLE_RATE = 2e6;
        public const int POINTS = 10;
        public const int HASH_RATE = 100;

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

        public unsafe static double CurveCorrelation(float* powerSrc, int len, double samplerate)
        {
            var lengthCurseFreqs = FftSharp.FFT.FrequencyScale(len, samplerate);
            var lengthCurseArray = new double[len];
            for (int i = 0; i < len - 1; i++) lengthCurseArray[i] = Math.Sqrt(Math.Pow(lengthCurseFreqs[i + 1] - lengthCurseFreqs[i], 2d) + Math.Pow(powerSrc[i + 1] - powerSrc[i], 2d));

            var lengthCurve = lengthCurseArray.Sum();
            var whiteNoiseLength = Math.Sqrt(Math.Pow(lengthCurseFreqs.Last() - lengthCurseFreqs.First(), 2d) + 1);

            var koef = (lengthCurve - whiteNoiseLength) / whiteNoiseLength;
            return koef;
        }

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

        public unsafe static void CumulativeSum(float* sequence, float* cumulativeSum, int length)
        {
            float sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += sequence[i];
                cumulativeSum[i] = sum;
            }
        }

        public unsafe static void IntegratedSpectrum(float* cumulativeSum, int length)
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

        public unsafe static void CompressArray(float* source, int sourceLength, float* compressed, int compressedLength)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (compressed == null)
                throw new ArgumentNullException(nameof(compressed));
            if (sourceLength <= 0)
                throw new ArgumentException("Source length must be positive.", nameof(sourceLength));
            if (compressedLength <= 0)
                throw new ArgumentException("Compressed length must be positive.", nameof(compressedLength));

            if (compressedLength >= sourceLength)
            {
                // Если сжатие не требуется — копируем (или обрезаем)
                int copyLen = Math.Min(sourceLength, compressedLength);
                for (int i = 0; i < copyLen; i++)
                {
                    compressed[i] = source[i];
                }
                // Остальную часть (если compressed длиннее) можно оставить как есть или обнулить — здесь не трогаем
                return;
            }

            float blockSize = (float)sourceLength / compressedLength;

            for (int i = 0; i < compressedLength; i++)
            {
                int start = (int)Math.Floor(i * blockSize);
                int end = (int)Math.Ceiling((i + 1) * blockSize);
                if (end > sourceLength) end = sourceLength;
                if (start >= end) start = end - 1; // защита от пустого блока

                float sum = 0f;
                int count = end - start;

                for (int j = start; j < end; j++)
                {
                    sum += source[j];
                }

                compressed[i] = sum / count;
            }
        }
    }
}
