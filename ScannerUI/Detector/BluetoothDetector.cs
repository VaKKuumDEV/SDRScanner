using InTheHand.Net.Sockets;
using ScannerUI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ScannerUI.Detector
{
    public class BluetoothDetector : IDetector
    {
        public string Name => "Bluetooth";
        private readonly List<string> _devices = [];

        private static readonly Lock _lock = new();
        private readonly Thread workerThread;
        private readonly CancellationTokenSource cts = new();

        public BluetoothDetector()
        {
            workerThread = new Thread(ProcessLoop)
            {
                IsBackground = true,
                Name = "BluetoothDetectorWorker"
            };
            workerThread.Start();
        }

        public DetectionResult[] Detect(IqBlock block, CancellationToken cancellation = default)
        {
            var freq = block.CenterFrequencyHz / 1e6;
            if (BluetoothChannelHelper.IsBluetoothFrequency(freq))
            {
                var channel = BluetoothChannelHelper.FreqToChannel(freq);
                return [.. _devices.Select(d => new DetectionResult()
                {
                    DetectorName = Name,
                    Label = d,
                })];
            }

            return [];
        }

        private void ProcessLoop()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    BluetoothClient client = new();
                    var devices = client.DiscoverDevices().ToList();

                    lock (_lock)
                    {
                        _devices.Clear();
                        _devices.AddRange(devices.Select(d => d.DeviceName));
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
