using SDRNet.Radio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Helpers
{
    public class FrameProcessor
    {
        private readonly double _fs = WiFiConstants.SampleRate;
        private readonly SDRNet.Radio.Complex[] _lltfFreq;
        private readonly int[] _dataBins = WiFiConstants.GetDataSubcarrierBins();
        private readonly int[] _pilotBins = WiFiConstants.GetPilotBins();
        private readonly ViterbiSoftDecoder _viterbi = new();

        public FrameProcessor()
        {
            _lltfFreq = WiFiConstants.BuildLltfFreqDomain();
        }

        public ScanResult? ProcessCapturedFrame(SDRNet.Radio.Complex[] iqSnippet, int channel, double rssiEstimateDb = double.NaN)
        {
            int ltfIndex = LocateLltf(iqSnippet);
            if (ltfIndex < 0) return null;

            int ltfPeriod = WiFiConstants.LtfSymSamples + WiFiConstants.CpLen;
            if (ltfIndex + ltfPeriod * 2 + 200 >= iqSnippet.Length) return null;

            int ltf_no_cp = WiFiConstants.LtfSymSamples;
            int ltfSym1 = ltfIndex + WiFiConstants.CpLen;
            SDRNet.Radio.Complex[] ltf1 = new SDRNet.Radio.Complex[ltf_no_cp];
            Array.Copy(iqSnippet, ltfSym1, ltf1, 0, ltf_no_cp);

            int ltfSym2 = ltfSym1 + ltf_no_cp + WiFiConstants.CpLen;
            if (ltfSym2 + ltf_no_cp > iqSnippet.Length) return null;
            SDRNet.Radio.Complex[] ltf2 = new SDRNet.Radio.Complex[ltf_no_cp];
            Array.Copy(iqSnippet, ltfSym2, ltf2, 0, ltf_no_cp);

            SDRNet.Radio.Complex[] LTF1_F = FFTSharpWrapper.FFT(ltf1);
            SDRNet.Radio.Complex[] LTF2_F = FFTSharpWrapper.FFT(ltf2);

            SDRNet.Radio.Complex[] H = new SDRNet.Radio.Complex[WiFiConstants.FFTSize];
            for (int k = 0; k < WiFiConstants.FFTSize; k++)
            {
                if (_lltfFreq[k].Modulus() > 0.0)
                {
                    SDRNet.Radio.Complex obs = (LTF1_F[k] + LTF2_F[k]) * 0.5f;
                    H[k] = obs / _lltfFreq[k];
                }
                else
                    H[k] = new SDRNet.Radio.Complex(0, 0);
            }

            int signalSymStart = ltfSym2 + ltf_no_cp + WiFiConstants.CpLen;
            if (signalSymStart + WiFiConstants.FFTSize > iqSnippet.Length) return null;

            SDRNet.Radio.Complex[] signalSymTime = new SDRNet.Radio.Complex[WiFiConstants.FFTSize];
            Array.Copy(iqSnippet, signalSymStart, signalSymTime, 0, WiFiConstants.FFTSize);
            SDRNet.Radio.Complex[] signalFreq = FFTSharpWrapper.FFT(signalSymTime);

            SDRNet.Radio.Complex[] eqSignal = new SDRNet.Radio.Complex[WiFiConstants.FFTSize];
            for (int k = 0; k < WiFiConstants.FFTSize; k++)
                eqSignal[k] = (H[k].Modulus() > 1e-6f) ? signalFreq[k] / H[k] : new SDRNet.Radio.Complex(0, 0);

            double noiseVar = EstimateNoiseVariance(signalFreq, H);

            double[] llrSignal = new double[48];
            for (int i = 0; i < _dataBins.Length; i++)
            {
                SDRNet.Radio.Complex val = eqSignal[_dataBins[i]];
                llrSignal[i] = (2.0 * val.Real) / (noiseVar + 1e-12);
            }

            var inter = new Interleaver80211(Ncbps: 48, Nbpsc: 1);
            double[] deintSig = inter.DeinterleaveSoft(llrSignal);
            int[] signalBits = _viterbi.Decode(deintSig);
            if (signalBits == null || signalBits.Length < 24) return null;

            int rateField = signalBits[0] | (signalBits[1] << 1) | (signalBits[2] << 2) | (signalBits[3] << 3);
            int lenField = 0;
            for (int i = 0; i < 12; i++) lenField |= (signalBits[4 + i] << i);
            if (lenField <= 0 || lenField > 4096) return null;

            int nBitsPayload = 16 + 8 * lenField + 6;
            int Ncbps = 48;
            int codedBitsPerSymbol = Ncbps * 2;
            int ofdmSymbolsPayload = (int)Math.Ceiling((double)nBitsPayload / Ncbps);

            List<double> payloadLlrPairs = new(ofdmSymbolsPayload * codedBitsPerSymbol);
            int symStart = signalSymStart + WiFiConstants.FFTSize + WiFiConstants.CpLen;

            for (int s = 0; s < ofdmSymbolsPayload; s++)
            {
                int pos = symStart + s * (WiFiConstants.FFTSize + WiFiConstants.CpLen);
                if (pos + WiFiConstants.FFTSize > iqSnippet.Length) break;
                SDRNet.Radio.Complex[] symTime = new SDRNet.Radio.Complex[WiFiConstants.FFTSize];
                Array.Copy(iqSnippet, pos + WiFiConstants.CpLen, symTime, 0, WiFiConstants.FFTSize);
                SDRNet.Radio.Complex[] symFreq = FFTSharpWrapper.FFT(symTime);

                SDRNet.Radio.Complex[] eq = new SDRNet.Radio.Complex[WiFiConstants.FFTSize];
                for (int k = 0; k < WiFiConstants.FFTSize; k++)
                    eq[k] = (H[k].Modulus() > 1e-6f) ? symFreq[k] / H[k] : new SDRNet.Radio.Complex(0, 0);

                SDRNet.Radio.Complex pilotErr = new(0, 0);
                foreach (int pb in _pilotBins)
                    pilotErr += eq[pb];

                double phaseCorrection = Math.Atan2(pilotErr.Imag, pilotErr.Real) / _pilotBins.Length;
                SDRNet.Radio.Complex rot = SDRNet.Radio.Complex.FromAngle(-phaseCorrection);
                for (int k = 0; k < eq.Length; k++)
                    eq[k] *= rot;

                foreach (int db in _dataBins)
                {
                    SDRNet.Radio.Complex v = eq[db];
                    double L = (2.0 * v.Real) / (noiseVar + 1e-12);
                    payloadLlrPairs.Add(L);
                    payloadLlrPairs.Add(L);
                }
            }

            double[] payloadLlrArray = [.. payloadLlrPairs];
            List<int> payloadBits = [];
            int idxPairs = 0;
            for (int sym = 0; sym < ofdmSymbolsPayload; sym++)
            {
                int pairCount = Ncbps * 2;
                if (idxPairs + pairCount > payloadLlrArray.Length) break;
                double[] pairsBlock = new double[pairCount];
                Array.Copy(payloadLlrArray, idxPairs, pairsBlock, 0, pairCount);
                idxPairs += pairCount;

                (int NbpscData, int NcbpsData) = rateField switch
                {
                    0b1101 or 0b1100 => (6, 288), // 64-QAM
                    0b1011 or 0b1010 => (4, 192), // 16-QAM
                    0b1001 or 0b1000 => (2, 96),  // QPSK
                    _ => (1, 48)                  // BPSK
                };
                var interData = new Interleaver80211(NcbpsData, NbpscData);
                double[] deintPairs = interData.DeinterleaveSoft(pairsBlock);
                var decoded = _viterbi.Decode(deintPairs);
                if (decoded == null) break;
                payloadBits.AddRange(decoded);
            }

            byte[]? payloadBytes = BitsToBytes([.. payloadBits]);
            if (payloadBytes == null || payloadBytes.Length < 36) return null;

            string bssid = BitConverter.ToString([.. payloadBytes.Skip(16).Take(6)]).Replace('-', ':');
            int bodyOffset = 24 + 12;
            if (payloadBytes.Length <= bodyOffset) return null;

            string? ssid = ParseSsidFromTagged(payloadBytes, bodyOffset);
            if (ssid == null) return null;

            return new ScanResult
            {
                SSID = ssid,
                BSSID = bssid,
                Channel = channel,
                RssiDbm = double.IsNaN(rssiEstimateDb) ? EstimateRssi(iqSnippet) : rssiEstimateDb,
                Timestamp = DateTime.UtcNow
            };
        }

        #region Helpers

        private int LocateLltf(SDRNet.Radio.Complex[] iq)
        {
            int best = -1;
            double bestVal = 0;
            int maxSearch = Math.Min(iq.Length - 128, (int)(_fs * 0.005));
            if (maxSearch <= 0) return -1;

            for (int i = 0; i < maxSearch; i += 4)
            {
                double corr = 0;
                int L = 64;
                for (int k = 0; k < L; k++)
                {
                    var a = iq[i + k];
                    var b = iq[i + k + L];
                    corr += a.Real * b.Real + a.Imag * b.Imag;
                }
                if (corr > bestVal)
                {
                    bestVal = corr;
                    best = i;
                }
            }
            return (bestVal > 1e-3) ? best : -1;
        }

        private static double EstimateNoiseVariance(SDRNet.Radio.Complex[] signalFreq, SDRNet.Radio.Complex[] H)
        {
            double sum = 0; int cnt = 0;
            for (int k = 0; k < signalFreq.Length; k++)
            {
                if (H[k].Modulus() < 1e-6f)
                {
                    sum += signalFreq[k].ModulusSquared();
                    cnt++;
                }
            }
            if (cnt == 0)
            {
                for (int k = 0; k < signalFreq.Length; k++)
                {
                    sum += signalFreq[k].ModulusSquared();
                    cnt++;
                }
            }
            return (sum / Math.Max(1, cnt)) + 1e-12;
        }

        private double EstimateRssi(SDRNet.Radio.Complex[] iq)
        {
            double sum = 0; int cnt = Math.Min(iq.Length, (int)(_fs * 0.002));
            for (int i = 0; i < cnt; i++) sum += iq[i].ModulusSquared();
            double avg = sum / Math.Max(1, cnt);
            return 10.0 * Math.Log10(avg + 1e-12);
        }

        private static byte[]? BitsToBytes(int[] bits)
        {
            if (bits == null || bits.Length < 8) return null;
            int n = bits.Length / 8;
            byte[] res = new byte[n];
            for (int i = 0; i < n; i++)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                    b |= (byte)((bits[i * 8 + j] & 1) << j);
                res[i] = b;
            }
            return res;
        }

        private static string? ParseSsidFromTagged(byte[] payload, int offset)
        {
            int pos = offset;
            while (pos + 2 <= payload.Length)
            {
                int id = payload[pos];
                int len = payload[pos + 1];
                pos += 2;
                if (pos + len > payload.Length) break;
                if (id == 0)
                {
                    try { return System.Text.Encoding.ASCII.GetString(payload, pos, len); }
                    catch { return null; }
                }
                pos += len;
            }
            return null;
        }

        #endregion
    }
}
