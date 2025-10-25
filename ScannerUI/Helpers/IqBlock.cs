using SDRNet.Radio;
using System;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Контейнер для блока IQ сэмплов и метаданных.
    /// </summary>
    public class IqBlock(float[] power, float noiseLevel, double sampleRateHz, double centerFrequencyHz)
    {
        public float[] Power { get; } = power;
        public float NoiseLevel { get; } = noiseLevel;
        public double SampleRateHz { get; } = sampleRateHz;
        public double CenterFrequencyHz { get; } = centerFrequencyHz;
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }
}
