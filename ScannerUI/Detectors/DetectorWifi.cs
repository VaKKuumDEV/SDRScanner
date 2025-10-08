using ScannerUI.Helpers;
using SDRNet.Radio;
using System;
using System.Linq;

namespace ScannerUI.Detectors
{
    /// <summary>
    /// detector: detects Wi-Fi-like bursts and extracts SSIDs via DecodeSsidsOptimized.
    /// </summary>
    /// <param name="stfCorrelationThreshold">threshold for STF autocorrelation to consider a burst Wi-Fi</param>
    /// <param name="minBurstLen">minimum IQ samples to process</param>
    public class DetectorWifi(float stfCorrelationThreshold = 0.25f, int minBurstLen = 1024) : IDetector
    {
        public string Name => "WiFi (802.11)";

        private readonly float stfCorrelationThreshold = stfCorrelationThreshold;
        private readonly int minBurstLen = Math.Max(256, minBurstLen);

        public DeviceDetection? AnalyzeAndRegister(Complex[] burstIq, double sampleRate, DetectorRegistration registration)
        {
            if (burstIq == null || burstIq.Length < minBurstLen)
                return null;

            int N = burstIq.Length;

            // Envelope statistics
            double[] env = new double[N];
            for (int i = 0; i < N; i++)
                env[i] = Math.Sqrt(burstIq[i].Real * burstIq[i].Real + burstIq[i].Imag * burstIq[i].Imag);

            double envMax = env.Max();
            double envMean = env.Average();
            bool strongEnvelope = (envMax / Math.Max(envMean, 1e-12) > 3.0);

            // Autocorrelation STF heuristic
            int lag = Math.Max(1, (int)Math.Round(0.8e-6 * sampleRate));
            if (lag >= N / 4) lag = Math.Max(1, N / 16);

            double corr = ComputeNormalizedAutocorrelation(burstIq, lag);
            double corr2 = ComputeNormalizedAutocorrelation(burstIq, Math.Min(N / 2 - 1, lag * 2));
            bool stfLike = corr > stfCorrelationThreshold || corr2 > stfCorrelationThreshold;

            // Quick spectrum estimate
            int fftSize = Tools.NextPowerOfTwo(Math.Min(32768, Math.Max(1024, N)));
            var avgMag = ComputeAvgMagSpectrum(burstIq, fftSize);
            double bw = Tools.EstimateBandwidthFromMag(avgMag, sampleRate, fftSize);
            bool bwLikely = bw > 2e6 && bw < 40e6;

            if (!(strongEnvelope && stfLike && bwLikely))
            {
                return null;
            }

            // --- Decode SSID using provided optimized method ---
            string[] ssids;
            try
            {
                ssids = WifiDecoder.DecodeSsidsOptimized(burstIq, sampleRate, 0.6);
            }
            catch
            {
                ssids = [];
            }

            // Register each SSID as a detected device
            foreach (var ssid in ssids)
            {
                string fingerprint = MakeFingerprint(ssid, bw, N / sampleRate);
                registration.Register(fingerprint, 0.0, bw, "802.11");
            }

            // Return DeviceDetection with SSID(s) if any, otherwise generic
            string title = ssids.Length > 0
                ? "WiFi: " + string.Join(", ", ssids.Distinct())
                : "WiFi (802.11-like)";

            string fpMain = ssids.Length > 0 ? MakeFingerprint(ssids[0], bw, N / sampleRate) : $"wifi|bw{Math.Round(bw / 1000) * 1000}|t{Math.Round(N / sampleRate, 3)}";

            return new DeviceDetection(title, fpMain, 0.0);
        }

        #region Helpers

        private static double ComputeNormalizedAutocorrelation(Complex[] x, int lag)
        {
            int N = x.Length;
            if (lag <= 0 || lag >= N) return 0.0;

            double re = 0.0, im = 0.0, denom = 0.0;
            for (int t = 0; t < N - lag; t++)
            {
                var a = x[t];
                var b = x[t + lag];
                re += (a.Real * b.Real + a.Imag * b.Imag);
                im += (a.Real * b.Imag - a.Imag * b.Real);
                denom += (a.Real * a.Real + a.Imag * a.Imag) * (b.Real * b.Real + b.Imag * b.Imag);
            }
            double mag = Math.Sqrt(re * re + im * im);
            return denom <= 0 ? 0.0 : mag / Math.Sqrt(denom);
        }

        private static double[] ComputeAvgMagSpectrum(Complex[] iq, int fftSize)
        {
            var native = new Complex[fftSize];
            int N = iq.Length;
            for (int i = 0; i < fftSize; i++) native[i] = i < N ? iq[i] : new Complex(0f, 0f);

            unsafe
            {
                fixed (Complex* p = native)
                {
                    Fourier.ForwardTransform(p, fftSize);
                }
            }

            int half = fftSize / 2;
            double[] mag = new double[half];
            for (int k = 0; k < half; k++) mag[k] = Math.Sqrt(native[k].Real * native[k].Real + native[k].Imag * native[k].Imag);
            return mag;
        }

        private static string MakeFingerprint(string ssid, double bw, double durationSec)
        {
            var bwR = Math.Round(bw / 1000.0) * 1000.0;
            var durR = Math.Round(durationSec, 3);
            return $"wifi|{ssid}|bw{bwR}|t{durR}";
        }

        #endregion
    }

}
