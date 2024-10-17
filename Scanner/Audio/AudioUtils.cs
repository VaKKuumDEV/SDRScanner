namespace Scanner.Audio
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

        public unsafe static List<int> GetNoiseFreqs(float* integratedSum, int length)
        {
            float scale = 1.0f / length;
            List<int> freqIndexes = [];

            for (int i = 0; i < length; i++)
            {
                if (Math.Abs(integratedSum[i] - scale * i) <= 1e-2F) freqIndexes.Add(i);
            }

            return freqIndexes;
        }
    }
}
