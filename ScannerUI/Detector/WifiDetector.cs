using ScannerUI.Helpers;
using SDRNet.Radio;
using System;
using System.Threading;

namespace ScannerUI.Detector
{
    public class WifiDetector : IDetector
    {
        public string Name => "WiFi";

        public DetectionResult[] Detect(Complex[] iq, DetectorContext context, CancellationToken cancellation = default)
        {
            var channel = WiFiChannelHelper.FreqToChannel(context.CenterFrequencyHz / 1e6);
            var detected = DetectWiFiSTF(iq, context.SampleRateHz);

            if (detected)
            {

            }

            return [];
        }

        private static bool DetectWiFiSTF(Complex[] iq, double fs)
        {
            if (iq == null || iq.Length == 0)
                return false;

            // Стандартная частота Wi-Fi и длина короткого символа
            const double stdFs = 20e6;  // 20 МГц
            const int shortSymbolStdSamples = 16;

            // Масштабируем длину одного короткого символа под текущую fs
            int shortSymbolSamples = (int)Math.Round(shortSymbolStdSamples * fs / stdFs);

            // STF состоит из 10 коротких символов
            int stfLength = 10 * shortSymbolSamples;

            if (iq.Length < stfLength) return false;

            // Инициализация для корреляции
            Complex sumCorr = new(0, 0);
            double sumPower = 0;

            // Корреляция повторяющихся коротких символов
            for (int i = 0; i < stfLength - shortSymbolSamples; i++)
            {
                Complex a = iq[i];
                Complex b = iq[i + shortSymbolSamples];
                sumCorr += a.Conjugate() * b;          // учитываем фазу
                sumPower += b.ModulusSquared();        // нормализация по мощности
            }

            double metric = sumCorr.Modulus() / Math.Sqrt(Math.Max(sumPower, 1e-12)); // нормализованная корреляция

            double threshold = shortSymbolSamples * 0.8; // порог можно подбирать эмпирически
            return metric > threshold;
        }
    }
}
