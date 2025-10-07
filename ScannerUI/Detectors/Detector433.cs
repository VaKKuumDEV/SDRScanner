using SDRNet.Radio;
using System.Linq;
using System;
using ScannerUI.Helpers;

namespace ScannerUI.Detectors
{
    /// <summary>
    /// Detector433: simple workflow:
    /// - compute envelope stats
    /// - compute an average spectrum of the burst
    /// - heuristics to decide OOK/ASK/FSK/CW
    /// - produce a fingerprint and register via DetectorRegistration
    /// </summary>
    public class Detector433 : IDetector
    {
        public string Name => "433MHz";

        public bool AnalyzeAndRegister(Complex[] burstIq, double sampleRate, DetectorRegistration registration)
        {
            if (burstIq == null || burstIq.Length < 16) return false; // too short

            // envelope
            int N = burstIq.Length;
            double[] env = new double[N];
            for (int i = 0; i < N; i++) env[i] = Math.Sqrt(burstIq[i].Real * burstIq[i].Real + burstIq[i].Imag * burstIq[i].Imag);
            double envMax = env.Max();
            double envMean = env.Average();
            double envStd = Math.Sqrt(env.Select(x => (x - envMean) * (x - envMean)).Sum() / Math.Max(1, N - 1));
            bool isOnOff = (envMax / Math.Max(envMean, 1e-12) > 3.0) && (envStd / Math.Max(envMean, 1e-12) > 0.5);

            // compute FFT (zero pad to next power of two within reasonable limit)
            int fftSize = Tools.NextPowerOfTwo(Math.Min(65536, Math.Max(1024, N)));
            var native = new Complex[fftSize];
            for (int i = 0; i < fftSize; i++) native[i] = i < N ? burstIq[i] : new Complex(0f, 0f);

            // call the same Fourier in-place (we have managed array; we need unsafe forward - reuse existing method if available)
            unsafe
            {
                fixed (Complex* p = native)
                {
                    Fourier.ForwardTransform(p, fftSize);
                    int half = fftSize / 2;
                    double[] avgMag = new double[half];
                    for (int k = 0; k < half; k++) avgMag[k] = Math.Sqrt(native[k].Real * native[k].Real + native[k].Imag * native[k].Imag);

                    var peaks = Tools.FindPeaks(avgMag, 3, 5.0);
                    double binWidth = sampleRate / fftSize;
                    var peakFreqs = peaks.Select(x => (x.BinIndex * binWidth) - sampleRate / 2.0).ToArray();
                    bool isFsk = peakFreqs.Length >= 2;

                    double centerFreq = 0;
                    if (peakFreqs.Length > 0) centerFreq = peakFreqs.OrderBy(f => Math.Abs(f)).First();
                    double bw = Tools.EstimateBandwidthFromMag(avgMag, sampleRate, fftSize);
                    double pulseRate = Tools.EstimatePulseRateFromEnvelope(env, sampleRate);

                    string mod = isFsk ? "FSK" : (isOnOff ? "OOK/ASK" : "CW/Other");

                    // here we only claim bursts around 433 MHz ± some margin, optionally
                    // Because LO is set by MainWindow, centerFreq is relative to LO; for 433 detector we accept any center near 0
                    // but if you want to restrict to 433.92, uncomment check below (and supply LO).
                    // double absoluteFreq = lo + centerFreq; // if you pass LO
                    // if (Math.Abs(absoluteFreq - 433_920_000) > 200_000) return false;

                    // fingerprint
                    string fp = MakeFingerprint(centerFreq, bw, mod, N / sampleRate, pulseRate);
                    registration.Register(fp, centerFreq, bw, mod);

                    return true; // recognized and registered
                }
            }
        }

        private static string MakeFingerprint(double centerFreq, double bw, string modType, double durationSec, double pulseRate)
        {
            double cf = Math.Round(centerFreq / 100.0) * 100.0; // 100 Hz bucket
            double bwR = Math.Round(bw / 50.0) * 50.0; // 50 Hz bucket
            double pr = Math.Round(pulseRate, 2);
            return $"{cf}|{bwR}|{modType}|{pr}";
        }
    }
}
