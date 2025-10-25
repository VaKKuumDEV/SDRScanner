using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;
using SDRNet.Radio;

namespace ScannerUI.Helpers
{
    public class BurstCollector : IDisposable
    {
        private readonly DetectorManager detectorManager;
        private readonly BlockingCollection<IqBlock> _queue;
        private readonly Action<IEnumerable<DetectionResult>> onBurstReady;

        private readonly Thread workerThread;
        private readonly AutoResetEvent dataEvent = new(false);
        private readonly CancellationTokenSource cts = new();

        public BurstCollector(DetectorManager detectorManager, Action<IEnumerable<DetectionResult>> onBurstReady, int maxQueueSize = 100)
        {
            this.detectorManager = detectorManager;
            this.onBurstReady = onBurstReady ?? throw new ArgumentNullException(nameof(onBurstReady));
            _queue = new BlockingCollection<IqBlock>(maxQueueSize);

            // Запуск фонового потока обработки
            workerThread = new Thread(ProcessLoop)
            {
                IsBackground = true,
                Name = "BurstCollectorWorker"
            };
            workerThread.Start();
        }

        /// <summary>
        /// Добавляет IQ-сэмплы для обработки.
        /// </summary>
        /// <param name="iq">Массив IQ-сэмплов (ваш Complex).</param>
        /// <param name="sampleRate">Частота дискретизации в Гц.</param>
        /// <param name="centerFrequency">Центральная частота блока в Гц.</param>
        public void ProcessIncoming(float[] power, float noiseLevel, double sampleRate, double centerFrequency)
        {
            if (power == null || power.Length == 0)
                throw new ArgumentException("Power array is null or empty", nameof(power));

            var block = new IqBlock(power, noiseLevel, sampleRate, centerFrequency);
            _queue.Add(block, cts.Token);
        }

        /// <summary>
        /// Фоновый цикл обработки IQ блоков.
        /// </summary>
        private void ProcessLoop()
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    if (_queue.Count == 0)
                    {
                        dataEvent.WaitOne(10);
                        continue;
                    }

                    foreach (var block in _queue.GetConsumingEnumerable(cts.Token))
                    {
                        var results = new List<DetectionResult>();

                        foreach (var detector in detectorManager.Detectors)
                        {
                            try
                            {
                                var detectorResults = detector.Detect(block, cts.Token);
                                if (detectorResults != null)
                                    results.AddRange(detectorResults);
                            }
                            catch (OperationCanceledException)
                            {
                                // Детектор корректно прерван
                            }
                            catch (Exception ex)
                            {
                                // Логировать или игнорировать ошибку конкретного детектора
                                Console.WriteLine($"Detector {detector.Name} error: {ex.Message}");
                            }
                        }

                        // Вызов события с результатами
                        onBurstReady?.Invoke([.. results]);
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
            dataEvent.Set();
            workerThread.Join(200);
            dataEvent.Dispose();
            cts.Dispose();
        }
    }
}
