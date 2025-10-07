using System.Collections.Generic;
using System;
using SDRNet.Radio;
using ScannerUI.Detectors;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Manages multiple detectors and central device registry.
    /// Single responsibility: route bursts to detectors and maintain global unique device list.
    /// </summary>
    public class DetectorManager
    {
        private readonly List<IDetector> detectors = new List<IDetector>();
        private readonly Dictionary<string, DeviceFingerprint> globalRegistry = new Dictionary<string, DeviceFingerprint>();

        public int TotalUniqueDevices => globalRegistry.Count;

        public void RegisterDetector(IDetector d) => detectors.Add(d);

        public void ProcessBurst(Complex[] burstIq, double sampleRate)
        {
            // route to each detector - detector may return true if it recognized the burst as its protocol
            foreach (var d in detectors)
            {
                try
                {
                    var reg = new DetectorRegistration(this, d.Name);
                    bool recognized = d.AnalyzeAndRegister(burstIq, sampleRate, reg);
                    // if recognized we still allow other detectors to look (some bursts might match multiple signatures)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Detector {d.Name} threw: {ex.Message}");
                }
            }
        }

        internal void RegisterDevice(string detectorName, string fingerprint, double centerFreqHz, double bandwidthHz, string modulation)
        {
            // key can include detectorName to avoid collisions across protocols
            string key = $"{detectorName}:{fingerprint}";
            if (!globalRegistry.ContainsKey(key))
            {
                globalRegistry[key] = new DeviceFingerprint
                {
                    DetectorName = detectorName,
                    Fingerprint = fingerprint,
                    CenterFrequencyHz = centerFreqHz,
                    BandwidthHz = bandwidthHz,
                    Modulation = modulation,
                    FirstSeen = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    Count = 1
                };
                Console.WriteLine($"New device ({detectorName}) #{globalRegistry.Count}: {fingerprint} f~{centerFreqHz:F0} Hz bw~{bandwidthHz:F0} Hz mod={modulation}");
            }
            else
            {
                var ex = globalRegistry[key];
                ex.LastSeen = DateTime.UtcNow;
                ex.Count++;
            }
        }
    }
}
