using System;

namespace ScannerUI.Helpers
{
    public class ScanResult
    {
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public double RssiDbm { get; set; }
        public int Channel { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
