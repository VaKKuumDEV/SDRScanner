using System;

namespace ScannerUI.Helpers
{
    // Полностью soft-decision Viterbi, constraint length 7, polynomials 171/133 (octal)
    public class ViterbiSoftDecoder
    {
        private readonly int _K = 7;
        private readonly int _states;
        private const double INF = 1e9;

        // polynomial taps (LSB = tap0)
        // g0 = 171 octal -> binary 1111001 (from LSB to MSB)
        // g1 = 133 octal -> binary 1011011
        private readonly int[] poly0 = [1, 0, 0, 1, 1, 1, 1]; // may adjust bit order
        private readonly int[] poly1 = [1, 1, 0, 1, 1, 0, 1];

        public ViterbiSoftDecoder()
        {
            _states = 1 << (_K - 1);
        }

        // llrPairs: contiguous LLRs for coded bits [b0, b1, b0, b1, ...], length = 2 * numSymbols
        // returns decoded hard bits (one bit per symbol)
        public int[] Decode(double[] llrPairs)
        {
            int n = llrPairs.Length / 2;
            double[] pathMetric = new double[_states];
            double[] newMetric = new double[_states];
            int[,] prevState = new int[n + 1, _states];
            int[,] prevInput = new int[n + 1, _states];

            for (int s = 0; s < _states; s++) pathMetric[s] = INF;
            pathMetric[0] = 0.0;

            for (int t = 0; t < n; t++)
            {
                for (int s = 0; s < _states; s++) newMetric[s] = INF;
                for (int s = 0; s < _states; s++)
                {
                    if (pathMetric[s] >= INF) continue;
                    for (int input = 0; input <= 1; input++)
                    {
                        int reg = ((input << (_K - 1)) | s) & ((1 << _K) - 1);
                        int out0 = ConvOut(reg, poly0);
                        int out1 = ConvOut(reg, poly1);
                        int next = reg >> 1;

                        // soft metric: prefer sign(llr) matching expected output
                        double metric = pathMetric[s]
                            - (out0 == 1 ? llrPairs[2 * t] : -llrPairs[2 * t])
                            - (out1 == 1 ? llrPairs[2 * t + 1] : -llrPairs[2 * t + 1]);

                        if (metric < newMetric[next])
                        {
                            newMetric[next] = metric;
                            prevState[t + 1, next] = s;
                            prevInput[t + 1, next] = input;
                        }
                    }
                }
                Array.Copy(newMetric, pathMetric, _states);
            }

            // pick best
            double best = INF; int bestState = 0;
            for (int s = 0; s < _states; s++) if (pathMetric[s] < best) { best = pathMetric[s]; bestState = s; }

            // backtrack
            int[] decoded = new int[n];
            int cur = bestState;
            for (int t = n; t >= 1; t--)
            {
                decoded[t - 1] = prevInput[t, cur];
                cur = prevState[t, cur];
            }
            return decoded;
        }

        private static int ConvOut(int reg, int[] poly)
        {
            int b = 0;
            for (int i = 0; i < poly.Length; i++) if (poly[i] != 0) b ^= (reg >> i) & 1;
            return b;
        }
    }
}
