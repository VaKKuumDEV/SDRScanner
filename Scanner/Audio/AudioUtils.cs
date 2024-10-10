namespace Scanner.Audio
{
    public static class AudioUtils
    {
        public const double SAMPLE_RATE = 2e6;
        public const int POINTS = 10;
        public const int HASH_RATE = 100;

        public struct Point(double x, double y, bool isLow = true)
        {
            public double X { get; } = x;
            public double Y { get; } = y;
            public bool IsLow { get; set; } = isLow;

            public override readonly string ToString()
            {
                return $"{X}:{Y}:{IsLow}";
            }
        };

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

        public static int GetBottomPoint(List<Point> points)
        {
            int minIndex = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[minIndex].Y > points[i].Y) minIndex = i;
                else break;
            }

            return minIndex;
        }

        public static int GetTopPoint(List<Point> points)
        {
            int maxIndex = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[maxIndex].Y < points[i].Y) maxIndex = i;
                else break;
            }

            return maxIndex;
        }
    }
}
