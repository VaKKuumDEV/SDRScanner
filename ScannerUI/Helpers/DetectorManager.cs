using ScannerUI.Detector;
using System;
using System.Collections.Generic;

namespace ScannerUI.Helpers
{
    public class DetectorManager : IDisposable
    {
        private readonly List<IDetector> detectors = [];
        public IDetector[] Detectors { get => [.. detectors]; }
        public void RegisterDetector(IDetector d) => detectors.Add(d);

        public void Dispose()
        {
            foreach (var detector in detectors)
            {
                detector.Dispose();
            }
        }
    }
}
