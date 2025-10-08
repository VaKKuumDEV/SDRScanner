using SDRNet.Radio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Energy-based burst detector working in a background thread.
    /// Uses a queue to offload heavy DSP work from UI thread.
    /// </summary>
    public class BurstCollector : IDisposable
    {
        private readonly RingBufferFloat noiseBuf;
        private readonly Action<Complex[], double> onBurstReady;
        private readonly float energyThresholdFactor = 6f;

        private readonly ConcurrentQueue<Complex[]> queue = new();
        private readonly Thread workerThread;
        private readonly AutoResetEvent dataEvent = new(false);
        private readonly CancellationTokenSource cts = new();

        private bool inBurst = false;
        private readonly List<Complex> currentBurst = [];
        private int silenceToEndSamples = 0;
        private int silenceCounter = 0;
        private double lastConfiguredSamplerate = 1e6; // default

        public BurstCollector(Action<Complex[], double> onBurstReady, int noiseBufSize = 10_000)
        {
            this.onBurstReady = onBurstReady ?? throw new ArgumentNullException(nameof(onBurstReady));
            noiseBuf = new(noiseBufSize);

            // Запуск фонового потока обработки
            workerThread = new Thread(ProcessLoop)
            {
                IsBackground = true,
                Name = "BurstCollectorWorker"
            };
            workerThread.Start();
        }

        public void ConfigureForSampleRate(double samplerate)
        {
            lastConfiguredSamplerate = samplerate;
            silenceToEndSamples = (int)(0.01 * samplerate); // 10 ms gap
        }

        /// <summary>
        /// Adds incoming IQ data to processing queue (non-blocking).
        /// </summary>
        public void ProcessIncoming(Complex[] iq)
        {
            if (iq == null || iq.Length == 0)
                return;

            queue.Enqueue(iq);
            dataEvent.Set(); // уведомляем поток о новых данных
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
                    if (queue.IsEmpty)
                    {
                        dataEvent.WaitOne(10);
                        continue;
                    }

                    if (queue.TryDequeue(out var iq))
                    {
                        ProcessBlock(iq);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Игнорируем завершение
            }
        }

        /// <summary>
        /// Обработка одного блока IQ (выполняется в фоне).
        /// </summary>
        private void ProcessBlock(Complex[] iq)
        {
            int n = iq.Length;
            float[] samplePower = new float[n];

            for (int i = 0; i < n; i++)
            {
                var c = iq[i];
                float p = c.Real * c.Real + c.Imag * c.Imag;
                samplePower[i] = p;
                noiseBuf.Push(p);
            }

            float mean = noiseBuf.Mean();
            float std = noiseBuf.StdDev(mean);
            float thresh = mean + energyThresholdFactor * std;

            for (int i = 0; i < n; i++)
            {
                float p = samplePower[i];
                if (!inBurst)
                {
                    if (p > thresh)
                    {
                        inBurst = true;
                        currentBurst.Clear();
                        currentBurst.Add(iq[i]);
                        silenceCounter = 0;
                    }
                }
                else
                {
                    currentBurst.Add(iq[i]);
                    if (p < thresh)
                    {
                        silenceCounter++;
                        if (silenceCounter > silenceToEndSamples)
                        {
                            var burst = currentBurst.ToArray();
                            try { onBurstReady?.Invoke(burst, lastConfiguredSamplerate); }
                            catch { /* ignore exceptions from callback */ }
                            inBurst = false;
                        }
                    }
                    else
                    {
                        silenceCounter = 0;
                    }
                }
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
