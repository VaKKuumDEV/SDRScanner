using SDRNet.Radio;
using System;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Контейнер для блока IQ сэмплов и метаданных.
    /// </summary>
    public class IqBlock(Complex[] samples, double sampleRateHz, double centerFrequencyHz)
    {
        public Complex[] Samples { get; } = samples;
        public double SampleRateHz { get; } = sampleRateHz;
        public double CenterFrequencyHz { get; } = centerFrequencyHz;
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }
}
