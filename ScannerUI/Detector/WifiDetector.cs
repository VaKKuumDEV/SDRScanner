using ScannerUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerUI.Detector
{
    public class WifiDetector : IDetector
    {
        public string Name => "WiFi";
        private readonly Dictionary<int, List<string>> devices = [];

        private static readonly Lock _lock = new();
        private readonly Thread workerThread;
        private readonly CancellationTokenSource cts = new();

        public WifiDetector()
        {
            workerThread = new Thread(ProcessLoop)
            {
                IsBackground = true,
                Name = "WiFiDetectorWorker"
            };
            workerThread.Start();
        }

        public DetectionResult[] Detect(IqBlock block, CancellationToken cancellation = default)
        {
            if (WiFiChannelHelper.IsWiFiFrequency(block.CenterFrequencyHz / 1e6))
            {
                var channel = WiFiChannelHelper.FreqToChannel(block.CenterFrequencyHz / 1e6);
                if (devices.TryGetValue(channel, out List<string>? value))
                {
                    return [.. value.Select(deviceAtChannel => new DetectionResult()
                    {
                        DetectorName = Name,
                        Label = deviceAtChannel,
                        Channel = channel,
                    })];
                }
            }

            return [];
        }

        private void ProcessLoop()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    Task.WaitAll(ManagedNativeWifi.NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10)));
                    var networks = ManagedNativeWifi.NativeWifi.EnumerateBssNetworks();

                    lock (_lock)
                    {
                        devices.Clear();
                        foreach (var network in networks)
                        {
                            if (!devices.ContainsKey(network.Channel)) devices[network.Channel] = [];
                            if (!devices[network.Channel].Contains(network.Ssid.ToString())) devices[network.Channel].Add(network.Ssid.ToString());
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Игнорируем завершение
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            workerThread.Join(200);
            cts.Dispose();
        }
    }
}
