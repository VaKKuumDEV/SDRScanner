using System;

namespace ScannerUI.Helpers
{
    public class DeviceDetection(string summary, string fingerprint, double frequency)
    {
        public string Summary { get; set; } = summary;
        public string Fingerprint { get; set; } = fingerprint;
        public double Frequency { get; set; } = frequency;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{Summary}: {Frequency:F0} Hz";
        }
    }
}
