namespace ScannerUI.Helpers
{
    /// <summary>
    /// Lightweight object that detectors use to register a found device (keeps central store in manager)
    /// </summary>
    public class DetectorRegistration(DetectorManager manager, string detectorName)
    {
        private readonly DetectorManager manager = manager;
        private readonly string detectorName = detectorName;

        public void Register(string fingerprint, double centerFreqHz, double bandwidthHz, string modulation)
        {
            manager.RegisterDevice(detectorName, fingerprint, centerFreqHz, bandwidthHz, modulation);
        }
    }
}
