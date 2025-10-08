using ScannerUI.Detector;
using SDRNet.Radio.PortAudio;
using System.Collections.Generic;

namespace ScannerUI.Helpers
{
    public class DetectorManager
    {
        private readonly List<IDetector> detectors = [];
        public IDetector[] Detectors { get => [.. detectors]; }

        public void RegisterDetector(IDetector d) => detectors.Add(d);
    }
}
