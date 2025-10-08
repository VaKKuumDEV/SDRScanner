using ScannerUI.Helpers;
using SDRNet.Radio;

namespace ScannerUI.Detectors
{
    public interface IDetector
    {
        /// <summary>Human-friendly name of detector (e.g. "433MHz")</summary>
        string Name { get; }

        /// <summary>Return true if this detector claims the burst (and optionally registers device)</summary>
        DeviceDetection? AnalyzeAndRegister(Complex[] burstIq, double sampleRate, DetectorRegistration registration);
    }
}
