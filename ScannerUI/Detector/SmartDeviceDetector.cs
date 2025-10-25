using ScannerUI.Audio;
using ScannerUI.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ScannerUI.Detector
{
    public class SmartDeviceDetector : IDetector
    {
        public const int BLOCK_LENGTH = 20;

        public static readonly Lock _lock = new();
        private readonly Thread workerThread;
        private readonly CancellationTokenSource cts = new();

        public string Name => "433 МГц";
        private readonly ConcurrentQueue<(string hash, int peaks)> _hashBlocks = new();
        private readonly List<DetectionResult> _detectionResults = [];

        public SmartDeviceDetector()
        {
            workerThread = new Thread(ProcessLoop)
            {
                IsBackground = true,
                Name = "SmartDeviceDetectorWorker"
            };
            workerThread.Start();
        }

        public unsafe DetectionResult[] Detect(IqBlock block, CancellationToken cancellation = default)
        {
            if (block.CenterFrequencyHz != (433 * 10e5)) return [];

            string hash;
            int peaks;
            fixed (float* blockSrc = block.Power)
            {
                (hash, peaks) = AudioUtils.CalculateSignalHash(blockSrc, block.Power.Length, block.NoiseLevel);
            }

            if (peaks > 2)
            {
                _hashBlocks.Enqueue((hash, peaks));
            }

            return [.. _detectionResults];
        }

        private void ProcessLoop()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    if (_hashBlocks.Count > BLOCK_LENGTH)
                    {
                        int diffs = _hashBlocks.Count - BLOCK_LENGTH;
                        for (int i = 0; i < diffs; i++)
                        {
                            if (_hashBlocks.TryDequeue(out var firstHash))
                            {
                                lock (_lock)
                                {
                                    var allHashes = _detectionResults.Select(r => (r.Label, r.Peaks));
                                    var minDifference = double.MaxValue;
                                    foreach (var (listHash, listHashPeaks) in allHashes)
                                    {
                                        if (Math.Abs(firstHash.peaks - listHashPeaks!.Value) > 1) continue;

                                        var difference = AudioUtils.CompareHashes(listHash!, firstHash.hash);
                                        if (difference < minDifference) minDifference = difference;
                                    }

                                    if (minDifference == double.MaxValue || minDifference > 0.3d) _detectionResults.Add(new()
                                    {
                                        DetectorName = Name,
                                        Label = firstHash.hash,
                                        Peaks = firstHash.peaks,
                                    });
                                }
                            }
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

            _detectionResults.Clear();
            _hashBlocks.Clear();
        }
    }
}
