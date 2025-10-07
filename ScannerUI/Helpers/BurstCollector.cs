using SDRNet.Radio;
using System;
using System.Collections.Generic;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Energy-based burst detector. Keeps noise floor estimate and emits completed bursts via callback.
    /// Single responsibility: collect bursts from stream of IQ samples.
    /// </summary>
    public class BurstCollector(Action<Complex[], double> onBurstReady, int noiseBufSize = 10_000)
    {
        private readonly RingBufferFloat noiseBuf = new(noiseBufSize);
        private readonly Action<Complex[], double> onBurstReady = onBurstReady ?? throw new ArgumentNullException(nameof(onBurstReady));
        private float energyThresholdFactor = 6f;
        private bool inBurst = false;
        private List<Complex> currentBurst = [];
        private int silenceToEndSamples = 0;
        private int silenceCounter = 0;
        private double lastConfiguredSamplerate = 1e6; // default

        public void ConfigureForSampleRate(double samplerate)
        {
            lastConfiguredSamplerate = samplerate;
            silenceToEndSamples = (int)(0.01 * samplerate); // 10 ms gap
        }

        public void ProcessIncoming(Complex[] iq)
        {
            if (iq == null || iq.Length == 0) return;

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

            // assemble bursts and when finished - callback
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
                            // completed burst
                            var burst = currentBurst.ToArray();
                            try { onBurstReady?.Invoke(burst, lastConfiguredSamplerate); }
                            catch { /* swallow exceptions from detectors to avoid stopping stream */ }
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
    }
}
