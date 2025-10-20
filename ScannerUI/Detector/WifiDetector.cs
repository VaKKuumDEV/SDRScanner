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

            return [];
        }
    }
}
