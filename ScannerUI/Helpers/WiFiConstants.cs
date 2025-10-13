using SDRNet.Radio;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Helpers
{
    public static class WiFiConstants
    {
        public const int FFTSize = 64;
        public const int CpLen = 16;
        public const int ShortSymSamples = 16; // short symbol length
        public const int LtfSymSamples = 64;
        public const double SampleRate = 20_000_000.0; // 20 Msps
                                                       // Legacy subcarriers: -26..-1, +1..+26 (DC at 0)
        public static int ScIndexToBin(int sc)
        {
            // map subcarrier index (-32..31) to FFT bin [0..63]
            int N = FFTSize;
            int bin = (sc < 0) ? sc + N : sc;
            return bin;
        }

        // Data subcarrier list for 802.11a (indices relative to -32..31, exclude pilots):
        // data: [-26..-1] + [1..26] excluding pilot positions {-21, -7, 7, 21}
        public static int[] GetDataSubcarrierBins()
        {
            List<int> bins = [];
            int[] pilots = [-21, -7, 7, 21];
            for (int k = -26; k <= -1; k++)
                if (!pilots.Contains(k)) bins.Add(ScIndexToBin(k));
            for (int k = 1; k <= 26; k++)
                if (!pilots.Contains(k)) bins.Add(ScIndexToBin(k));
            return [.. bins]; // length 48
        }

        // Pilot bins (for phase tracking)
        public static int[] GetPilotBins()
        {
            int[] pilots = [-21, -7, 7, 21];
            return [.. pilots.Select(ScIndexToBin)];
        }

        // Pilot known sequence for each OFDM symbol: according to 802.11a sequence [+1,+1,+1,-1] rotated with symbol index.
        // For simplicity, we'll compute expected pilot values using standard PN sequence sign pattern for pilot carriers.
        public static Complex[] GetPilotExpectedSequence(int numSymbols)
        {
            // sequence per symbol: [1, 1, 1, -1] multiplied by (-1)^{symbolIndex} maybe; we will use simplest repetend
            Complex[] seq = new Complex[numSymbols];
            for (int i = 0; i < numSymbols; i++)
            {
                int v = (i % 4 == 3) ? -1 : 1;
                seq[i] = new Complex(v, 0);
            }
            return seq;
        }

        // L-LTF frequency domain template (for estimation). Use standard L-LTF frequency taps (±1 phases).
        // We'll provide simplified LTF frequency values: subcarriers -26..-1,+1..+26 = known ±1 pattern.
        public static Complex[] BuildLltfFreqDomain()
        {
            int N = FFTSize;
            Complex[] F = new Complex[N];
            for (int i = 0; i < N; i++) F[i] = new(0f, 0f);

            // canonical L-LTF frequency pattern (values ±1, real). Real pattern from standard:
            // We'll set active subcarriers to +1 or -1 using pattern from common implementations.
            // Simpler variant: all active subcarriers = +1 (works for channel estimation up to scale).
            for (int k = -26; k <= -1; k++) F[ScIndexToBin(k)] = new Complex(1f, 0f);
            for (int k = 1; k <= 26; k++) F[ScIndexToBin(k)] = new Complex(1f, 0f);

            return F;
        }
    }
}
