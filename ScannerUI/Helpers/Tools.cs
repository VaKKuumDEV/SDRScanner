using System.Collections.Generic;
using System.Linq;
using System;

namespace ScannerUI.Helpers
{
    public static class Tools
    {
        public static int NextPowerOfTwo(int v)
        {
            int p = 1;
            while (p < v) p <<= 1;
            return p;
        }

        public struct PeakInfo { public int BinIndex; public double Value; }

        public static PeakInfo[] FindPeaks(double[] mag, int maxPeaks, double minProminence)
        {
            var peaks = new List<PeakInfo>();
            int n = mag.Length;
            for (int i = 1; i < n - 1; i++)
            {
                if (mag[i] > mag[i - 1] && mag[i] > mag[i + 1])
                {
                    double leftMax = 0, rightMax = 0;
                    for (int l = Math.Max(0, i - 20); l < i; l++) leftMax = Math.Max(leftMax, mag[l]);
                    for (int r = i + 1; r < Math.Min(n, i + 20); r++) rightMax = Math.Max(rightMax, mag[r]);
                    double prominence = mag[i] - Math.Max(leftMax, rightMax);
                    if (prominence >= minProminence) peaks.Add(new PeakInfo { BinIndex = i, Value = mag[i] });
                }
            }
            return peaks.OrderByDescending(p => p.Value).Take(maxPeaks).ToArray();
        }

        public static double EstimateBandwidthFromMag(double[] mag, double sampleRate, int fftSize)
        {
            int bins = mag.Length;
            double[] avg = mag;
            int peak = 0; double max = avg[0];
            for (int k = 1; k < bins; k++) if (avg[k] > max) { max = avg[k]; peak = k; }
            double level = max * 0.5;
            int left = peak, right = peak;
            while (left > 0 && avg[left] > level) left--;
            while (right < bins - 1 && avg[right] > level) right++;
            double bwHz = (right - left) * (sampleRate / fftSize);
            return bwHz;
        }

        public static double EstimatePulseRateFromEnvelope(double[] env, double sampleRate)
        {
            int N = env.Length;
            int maxLag = Math.Min(N / 4, (int)(sampleRate * 0.5));
            if (maxLag < 10) return 0;
            double mean = env.Average();
            double[] ac = new double[maxLag];
            for (int lag = 1; lag < maxLag; lag++)
            {
                double s = 0;
                for (int i = 0; i + lag < N; i++) s += (env[i] - mean) * (env[i + lag] - mean);
                ac[lag] = s;
            }
            int best = 1; double bestv = ac[1];
            for (int i = 2; i < maxLag; i++) if (ac[i] > bestv) { bestv = ac[i]; best = i; }
            if (bestv <= 0) return 0;
            return sampleRate / best;
        }
    }
}
