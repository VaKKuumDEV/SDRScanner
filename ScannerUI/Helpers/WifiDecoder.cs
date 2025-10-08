using SDRNet.Radio;
using System;
using System.Collections.Generic;

namespace ScannerUI.Helpers
{
    public static class WifiDecoder
    {
        public static string[] DecodeSsidsOptimized(Complex[] iq, double sampleRate, double detectThreshold = 0.6)
        {
            if (iq == null || iq.Length < 1024) return [];

            int shortLen = Math.Max(4, (int)Math.Round(sampleRate * 0.8e-6));
            int stfWindow = shortLen * 10;
            var candidates = FindStfCandidates(iq, shortLen, stfWindow, detectThreshold);

            var foundSsids = new HashSet<string>();

            foreach (int idx in candidates)
            {
                try
                {
                    var bits = DemodulateOfdmPacketInPlace(iq, idx, sampleRate);
                    if (bits == null || bits.Length == 0) continue;

                    var payload = BitsToBytes(bits);
                    var ssid = ParseSsidsFromBeacon(payload);
                    if (!string.IsNullOrEmpty(ssid)) foundSsids.Add(ssid);
                }
                catch { }
            }

            return [.. foundSsids];
        }

        #region STF detection
        static List<int> FindStfCandidates(Complex[] iq, int L, int stfWindow, double threshold)
        {
            int N = iq.Length;
            var peaks = new List<int>();
            if (N < stfWindow + L) return peaks;

            double[] power = new double[N];
            for (int i = 0; i < N; i++) power[i] = iq[i].ModulusSquared();

            for (int n = 0; n + stfWindow + L < N; n += Math.Max(1, L / 2))
            {
                Complex acc = new(0f, 0f);
                double accPower = 0;
                for (int k = 0; k < stfWindow; k++)
                {
                    acc += iq[n + k] * ~iq[n + k + L];
                    accPower += power[n + k] + power[n + k + L];
                }
                double mag = acc.Modulus();
                double norm = Math.Sqrt(Math.Max(1e-12, accPower));
                double val = mag / norm;
                if (val > threshold) peaks.Add(n);
            }

            peaks.Sort();
            var filtered = new List<int>();
            int minSep = Math.Max(1, stfWindow / 2);
            int last = -minSep * 2;
            foreach (var p in peaks)
            {
                if (p - last > minSep)
                {
                    filtered.Add(p);
                    last = p;
                }
            }
            return filtered;
        }
        #endregion

        #region OFDM demodulation (без копирования IQ)
        static byte[]? DemodulateOfdmPacketInPlace(Complex[] iq, int startIdx, double sampleRate)
        {
            int fftSize = 64;
            int cp = Math.Max(1, (int)Math.Round(sampleRate * 0.8e-6));
            int ofdmSymbol = fftSize + cp;

            if (startIdx + 3 * ofdmSymbol >= iq.Length) return null;

            // LTF
            var ltfSym1 = new Complex[fftSize];
            var ltfSym2 = new Complex[fftSize];
            Array.Copy(iq, startIdx + cp, ltfSym1, 0, fftSize);
            Array.Copy(iq, startIdx + ofdmSymbol + cp, ltfSym2, 0, fftSize);

            var LTF1 = FFT(ltfSym1);
            var LTF2 = FFT(ltfSym2);
            var LTFavg = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++) LTFavg[i] = (LTF1[i] + LTF2[i]) / 2f;

            var refLtf = GenerateReferenceLtf(fftSize);
            var H = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++)
                H[i] = refLtf[i].ModulusSquared() < 1e-12 ? new Complex(0f, 0f) : LTFavg[i] / refLtf[i];

            int sigOffset = startIdx + 2 * ofdmSymbol;
            if (sigOffset + ofdmSymbol >= iq.Length) return null;

            int payloadSymbols = 8;
            int payloadOffset = sigOffset + ofdmSymbol;
            var allBits = new List<byte>();
            for (int sym = 0; sym < payloadSymbols; sym++)
            {
                int off = payloadOffset + sym * ofdmSymbol;
                if (off + ofdmSymbol >= iq.Length) break;

                var symTime = new Complex[fftSize];
                Array.Copy(iq, off + cp, symTime, 0, fftSize);
                var symFreq = FFT(symTime);
                for (int i = 0; i < fftSize; i++)
                    if (H[i].ModulusSquared() > 1e-12) symFreq[i] = symFreq[i] / H[i];

                var symBits = DemapBpskFromOfdm(symFreq, fftSize);
                if (symBits != null && symBits.Length > 0) allBits.AddRange(symBits);
            }

            if (allBits.Count == 0) return null;
            var recvBits = allBits.ToArray();
            var decoded = ViterbiDecodeFull(recvBits);
            return decoded;
        }

        static double Energy(Complex[] arr, int off, int len)
        {
            double e = 0;
            for (int i = 0; i < len && off + i < arr.Length; i++)
                e += arr[off + i].ModulusSquared();
            return e;
        }

        static Complex[] GenerateReferenceLtf(int fftSize)
        {
            var res = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                int k = i - fftSize / 2;
                res[i] = k == 0 ? new Complex(0f, 0f) : ((k % 2 == 0) ? new Complex(1f, 0f) : new Complex(-1f, 0f));
            }
            return res;
        }

        static byte[] DemapBpskFromOfdm(Complex[] freq, int fftSize)
        {
            var bits = new List<byte>();
            int dc = fftSize / 2;
            for (int i = 0; i < fftSize; i++)
            {
                if (i == dc) continue;
                bits.Add((byte)(freq[i].Real >= 0 ? 1 : 0));
            }
            return [.. bits];
        }

        static Complex[] FFT(Complex[] x)
        {
            int n = x.Length;
            Complex[] a = new Complex[n];
            Array.Copy(x, a, n);
            int bits = (int)Math.Log(n, 2);
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, bits);
                if (j > i) {
                    (a[j], a[i]) = (a[i], a[j]);
                }
            }
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = -2.0 * Math.PI / len;
                Complex wlen = new((float)Math.Cos(angle), (float)Math.Sin(angle));
                for (int i = 0; i < n; i += len)
                {
                    Complex w = new(1f, 0f);
                    for (int j = 0; j < len / 2; j++)
                    {
                        Complex u = a[i + j];
                        Complex v = a[i + j + len / 2] * w;
                        a[i + j] = u + v;
                        a[i + j + len / 2] = u - v;
                        w *= wlen;
                    }
                }
            }
            return a;
        }

        static int ReverseBits(int val, int bits)
        {
            int res = 0;
            for (int i = 0; i < bits; i++)
            {
                res = (res << 1) | (val & 1);
                val >>= 1;
            }
            return res;
        }
        #endregion

        #region Full Viterbi decoder (K=7, rate 1/2)
        static byte[] ViterbiDecodeFull(byte[] codedBits)
        {
            if (codedBits.Length < 2) return [];
            int N = codedBits.Length / 2;

            int K = 7;
            int nStates = 1 << (K - 1);
            int[,] nextState = new int[nStates, 2];
            byte[,] output = new byte[nStates, 2];

            int g0 = 0b1011011;
            int g1 = 0b1111001;

            for (int s = 0; s < nStates; s++)
            {
                for (int b = 0; b <= 1; b++)
                {
                    int next = ((s << 1) | b) & (nStates - 1);
                    nextState[s, b] = next;
                    int out0 = Parity((s << 1 | b) & g0);
                    int out1 = Parity((s << 1 | b) & g1);
                    output[s, b] = (byte)((out0 << 1) | out1);
                }
            }

            double[] pathMetric = new double[nStates];
            int[][] paths = new int[nStates][];
            for (int i = 0; i < nStates; i++) paths[i] = new int[N];

            for (int t = 0; t < N; t++)
            {
                double[] newMetric = new double[nStates];
                int[][] newPaths = new int[nStates][];
                for (int i = 0; i < nStates; i++) newPaths[i] = new int[N];

                for (int s = 0; s < nStates; s++)
                {
                    for (int b = 0; b <= 1; b++)
                    {
                        int ns = nextState[s, b];
                        byte outBits = output[s, b];
                        double metric = pathMetric[s];
                        int bit0 = codedBits[2 * t];
                        int bit1 = codedBits[2 * t + 1];
                        metric += HammingDistance(outBits, bit0, bit1);

                        if (newPaths[ns][t] == 0 || metric < newMetric[ns])
                        {
                            newMetric[ns] = metric;
                            Array.Copy(paths[s], newPaths[ns], t);
                            newPaths[ns][t] = b;
                        }
                    }
                }
                pathMetric = newMetric;
                paths = newPaths;
            }

            double bestMetric = double.MaxValue;
            int bestState = 0;
            for (int s = 0; s < nStates; s++)
            {
                if (pathMetric[s] < bestMetric)
                {
                    bestMetric = pathMetric[s];
                    bestState = s;
                }
            }

            var decoded = new byte[N];
            for (int i = 0; i < N; i++)
                decoded[i] = (byte)paths[bestState][i];

            return decoded;
        }

        static int Parity(int x)
        {
            x ^= x >> 4;
            x ^= x >> 2;
            x ^= x >> 1;
            return x & 1;
        }

        static int HammingDistance(byte outBits, int b0, int b1)
        {
            int d = 0;
            if (((outBits >> 1) & 1) != b0) d++;
            if ((outBits & 1) != b1) d++;
            return d;
        }
        #endregion

        #region Bits/Bytes and Beacon parsing
        static byte[] BitsToBytes(byte[] bits)
        {
            int len = bits.Length / 8;
            if (len == 0) return [];
            var res = new byte[len];
            for (int i = 0; i < len; i++)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                    b = (byte)((b << 1) | (bits[i * 8 + j] & 1));
                res[i] = b;
            }
            return res;
        }

        static string? ParseSsidsFromBeacon(byte[] payload)
        {
            if (payload == null || payload.Length < 36) return null;
            int pos = 36;
            while (pos + 2 <= payload.Length)
            {
                int id = payload[pos];
                int len = payload[pos + 1];
                pos += 2;
                if (pos + len > payload.Length) break;
                if (id == 0)
                {
                    if (len == 0) return "";
                    var ssidBytes = new byte[len];
                    Array.Copy(payload, pos, ssidBytes, 0, len);
                    try { return System.Text.Encoding.ASCII.GetString(ssidBytes); } catch { return null; }
                }
                pos += len;
            }
            return null;
        }
        #endregion
    }
}
