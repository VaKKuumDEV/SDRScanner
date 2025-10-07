using System;

namespace ScannerUI.Helpers
{
    public class DeviceFingerprint
    {
        public string DetectorName { get; set; } = "";
        public string Fingerprint { get; set; } = "";
        public double CenterFrequencyHz { get; set; }
        public double BandwidthHz { get; set; }
        public string Modulation { get; set; } = "";
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public int Count { get; set; }
    }
}
