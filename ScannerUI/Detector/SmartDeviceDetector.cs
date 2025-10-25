using Avalonia.Media;
using ScannerUI.Audio;
using ScannerUI.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ScannerUI.Detector
{
    public class SmartDeviceDetector : IDetector
    {
        public string Name => "433 МГц";
        private readonly ConcurrentQueue<string> _hashBlocks = new();

        public unsafe DetectionResult[] Detect(IqBlock block, CancellationToken cancellation = default)
        {
            if (block.CenterFrequencyHz != (433 * 10e5)) return [];

            string hash;
            int peaks;
            fixed (float* blockSrc = block.Power)
            {
                (hash, peaks) = AudioUtils.CalculateSignalHash(blockSrc, block.Power.Length, block.NoiseLevel);
            }

            if (peaks > 2) _hashBlocks.Enqueue(hash);
            if (_hashBlocks.Count > 20)
            {
                _hashBlocks.TryDequeue(out _);
            }

            var list = new List<DetectionResult>();
            return [.. list];
        }

        public void Dispose()
        {
            _hashBlocks.Clear();
        }
    }
}
