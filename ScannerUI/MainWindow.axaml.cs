using Avalonia.Controls;
using Avalonia.Threading;
using SDRNet.Radio;
using SDRNet.HackRfOne;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScannerUI
{
    // NOTE: This file replaces and extends your MainWindow logic with
    // - energy-based burst detection
    // - simple spectral classification (FSK / OOK / CW)
    // - fingerprinting + counting unique devices
    // It relies on the same types you already use (Complex, Fourier, HackRFIO, DeviceDisplay).

    public partial class MainWindow : Window
    {
        public const int Resolution = 131072;

        // UI-bound properties
        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private double[] FrequesList { get; set; } = new double[Resolution];
        public List<DeviceDisplay> Devices { get; set; } = [];
        public HackRFIO? IO { get; set; } = null;

        // Detection state
        private RingBufferFloat noiseBuf = new(10_000);
        private float energyThresholdFactor = 6f; // K * std
        private bool inBurst = false;
        private List<Complex> currentBurst = [];
        private int silenceToEndSamples = 0; // initialized on Start based on samplerate
        private int silenceCounter = 0;

        // Found devices
        private HashSet<string> fingerprints = [];

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        public MainWindow()
        {
            InitializeComponent();

            DevicesBox.SelectionChanged += new((sender, args) =>
            {
                int index = DevicesBox.SelectedIndex;
                if (index != -1 && index < Devices.Count)
                {
                    DeviceDisplay device = Devices[index];
                    IO = new HackRFIO();
                    IO.SelectDevice(device.Index);

                    Status = WorkingStatuses.STOPPED;
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });

            Unloaded += new((sender, args) =>
            {
                Stop();
            });

            LoadDevicesList();

            SpectrPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            PoweredPlot.Plot.Axes.Frameless();
            PoweredPlot.Refresh();
        }

        private void LoadDevicesList()
        {
            DeviceDisplay[] devices = DeviceDisplay.GetActiveDevices();
            Devices.Clear();
            Devices.AddRange(devices);

            DevicesBox.Items.Clear();
            foreach (DeviceDisplay device in Devices) DevicesBox.Items.Add("(#" + device.Index + ") " + device.Name);

            if (Devices.Count > 0) DevicesBox.SelectedIndex = 0;
        }

        private void Stop()
        {
            IO?.Stop();

            Status = WorkingStatuses.STOPPED;
            ControlButton.Content = "Запуск";
        }

        private unsafe void Start()
        {
            if (IO != null)
            {
                uint freq = Frequency * 1000;

                // prepare frequency axis for plot
                double fSampleRate = FftSharp.FFT.FrequencyResolution(Resolution, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (fSampleRate * Resolution / 2) + (i * fSampleRate);

                IO.Frequency = freq;

                // silence termination set to 10 ms by default
                silenceToEndSamples = (int)(0.01 * IO.Samplerate);

                IO.Start(IO_SamplesAvailable);

                ControlButton.Content = "Остановить";
                SpectrPlot.Plot.Clear();

                Status = WorkingStatuses.STARTED;
            }
        }

        // ---------------------- Core callback and detection ----------------------
        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;

            // Make a managed copy of the incoming IQ (we must not destroy time-domain data before detection)
            Complex[] iq = new Complex[len];
            for (int i = 0; i < len; i++) iq[i] = data[i];

            // 1) Update noise estimate and run energy detector
            float[] samplePower = new float[len];
            for (int i = 0; i < len; i++)
            {
                var c = iq[i];
                float p = c.Real * c.Real + c.Imag * c.Imag; // |IQ|^2
                samplePower[i] = p;
                noiseBuf.Push(p);
            }

            float mean = noiseBuf.Mean();
            float std = noiseBuf.StdDev(mean);
            float thresh = mean + energyThresholdFactor * std;

            // 2) Burst collection using threshold
            ProcessSamplesForBursts(iq, samplePower, thresh);

            // 3) Compute a spectrum for visualization only (non-destructive)
            // We'll copy IQ again to native buffer and call Fourier.* methods as before
            fixed (Complex* p = iq)
            {
                // copy into native buffer to avoid changing 'iq' that may be used elsewhere
                Complex* tmp = stackalloc Complex[len];
                for (int i = 0; i < len; i++) tmp[i] = p[i];

                // transform and compute power
                Fourier.ForwardTransform(tmp, len);
                float[] power = new float[len];
                fixed (float* pp = power)
                {
                    Fourier.SpectrumPower(tmp, pp, len);
                }

                // UI update
                Dispatcher.UIThread.Post(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(FrequesList, power);

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.AutoScaleY();
                    SpectrPlot.Refresh();

                    // show found devices count
                    CorellationBox.Text = $"Уникальных устройств: {fingerprints.Count}";
                });
            }
        }

        private void ProcessSamplesForBursts(Complex[] iq, float[] samplePower, float thresh)
        {
            int n = iq.Length;
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
                            // end of burst
                            var burst = currentBurst.ToArray();
                            AnalyzeBurst(burst, IO!.Samplerate);
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

        // ---------------------- Burst analysis & classification ----------------------
        private unsafe void AnalyzeBurst(Complex[] burstIq, double sampleRate)
        {
            // Basic features: duration, envelope stats, average spectrum
            int N = burstIq.Length;
            double durationSec = N / sampleRate;

            // Envelope
            double[] env = new double[N];
            for (int i = 0; i < N; i++) env[i] = Math.Sqrt(burstIq[i].Real * burstIq[i].Real + burstIq[i].Imag * burstIq[i].Imag);
            double envMax = env.Max();
            double envMean = env.Average();
            double envStd = Math.Sqrt(env.Select(x => (x - envMean) * (x - envMean)).Sum() / Math.Max(1, N - 1));

            bool isOnOff = (envMax / Math.Max(envMean, 1e-12) > 3.0) && (envStd / Math.Max(envMean, 1e-12) > 0.5);

            // Average spectrum via single FFT on zero-padded burst
            int fftSize = NextPowerOfTwo(Math.Min(65536, Math.Max(1024, N)));
            Complex* native = stackalloc Complex[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                if (i < N) native[i] = burstIq[i];
                else native[i] = new Complex(0f, 0f);
            }

            Fourier.ForwardTransform(native, fftSize);
            // compute power (positive half)
            int half = fftSize / 2;
            double[] avgMag = new double[half];
            for (int k = 0; k < half; k++) avgMag[k] = Math.Sqrt(native[k].Real * native[k].Real + native[k].Imag * native[k].Imag);

            // find peaks in avgMag
            var peaks = FindPeaks(avgMag, 3, 5.0); // up to 3 peaks, minProminence 5
            double binWidth = sampleRate / fftSize;
            var peakFreqs = peaks.Select(p => (p.BinIndex * binWidth) - sampleRate / 2.0).ToArray();

            bool isFsk = peakFreqs.Length >= 2;

            // estimate center and bandwidth
            double centerFreq = 0;
            if (peakFreqs.Length > 0) centerFreq = peakFreqs.OrderBy(f => Math.Abs(f)).First(); // nearest to center (LO)
            double bw = EstimateBandwidthFromMag(avgMag, sampleRate, fftSize);

            double pulseRate = EstimatePulseRateFromEnvelope(env, sampleRate);

            string mod = isFsk ? "FSK" : (isOnOff ? "OOK/ASK" : "CW/Other");

            string fp = MakeFingerprint(centerFreq, bw, mod, durationSec, pulseRate);

            RegisterFingerprint(fp, centerFreq, bw, mod);

            // Optionally: log to UI
            Dispatcher.UIThread.Post(() =>
            {
                // Append a short entry to a TextBox or status area if exists
                var entry = $"{DateTime.Now:HH:mm:ss} | f~{centerFreq:F0} Hz | bw~{bw:F0} Hz | mod={mod} | dur={durationSec * 1000:F0} ms";
                //DetectionLog.Text = entry + "\n" + DetectionLog.Text;
                // Update detected count
                CorellationBox.Text = $"Уникальных устройств: {fingerprints.Count}";
            });
        }

        private static double EstimatePulseRateFromEnvelope(double[] env, double sampleRate)
        {
            // crude: compute autocorrelation of envelope and find first non-zero lag peak -> period
            int N = env.Length;
            int maxLag = Math.Min(N / 4, (int)(sampleRate * 0.5));
            if (maxLag < 10) return 0;
            double mean = env.Average();
            double[] ac = new double[maxLag];
            for (int lag = 1; lag < maxLag; lag++)
            {
                double s = 0;
                for (int i = 0; i + lag < N; i++) s += (env[i] - mean) * (env[i + lag] - mean);
                ac[lag] = s;
            }
            int best = 1; double bestv = ac[1];
            for (int i = 2; i < maxLag; i++) if (ac[i] > bestv) { bestv = ac[i]; best = i; }
            if (bestv <= 0) return 0;
            return sampleRate / best; // pulses per second
        }

        private static double EstimateBandwidthFromMag(double[] mag, double sampleRate, int fftSize)
        {
            int bins = mag.Length;
            double[] avg = mag; // already magnitude
            int peak = 0; double max = avg[0];
            for (int k = 1; k < bins; k++) if (avg[k] > max) { max = avg[k]; peak = k; }
            double level = max * 0.5; // -6dB approx
            int left = peak, right = peak;
            while (left > 0 && avg[left] > level) left--;
            while (right < bins - 1 && avg[right] > level) right++;
            double bwHz = (right - left) * (sampleRate / fftSize);
            return bwHz;
        }

        private struct PeakInfo { public int BinIndex; public double Value; }

        private static PeakInfo[] FindPeaks(double[] mag, int maxPeaks, double minProminence)
        {
            List<PeakInfo> peaks = [];
            int n = mag.Length;
            for (int i = 1; i < n - 1; i++)
            {
                if (mag[i] > mag[i - 1] && mag[i] > mag[i + 1])
                {
                    double leftMax = 0, rightMax = 0;
                    for (int l = Math.Max(0, i - 20); l < i; l++) leftMax = Math.Max(leftMax, mag[l]);
                    for (int r = i + 1; r < Math.Min(n, i + 20); r++) rightMax = Math.Max(rightMax, mag[r]);
                    double prominence = mag[i] - Math.Max(leftMax, rightMax);
                    if (prominence >= minProminence) peaks.Add(new PeakInfo { BinIndex = i, Value = mag[i] });
                }
            }
            return peaks.OrderByDescending(p => p.Value).Take(maxPeaks).ToArray();
        }

        private static string MakeFingerprint(double centerFreq, double bw, string modType, double durationSec, double pulseRate)
        {
            // round to tolerate small drifts
            double cf = Math.Round(centerFreq / 100.0) * 100.0; // round to 100 Hz
            double bwR = Math.Round(bw / 50.0) * 50.0; // 50 Hz
            double pr = Math.Round(pulseRate, 2);
            return $"{cf}|{bwR}|{modType}|{pr}";
        }

        private void RegisterFingerprint(string fp, double centerFreq, double bw, string mod)
        {
            if (fingerprints.Add(fp))
            {
                Console.WriteLine($"New device #{fingerprints.Count}: {fp}  (f~{centerFreq:F0}Hz, bw~{bw:F0}Hz, mod={mod})");
            }
            else
            {
                // existing device - could update last seen/time/count
            }
        }

        // ---------------------- Small helpers ----------------------
        private static int NextPowerOfTwo(int v)
        {
            int p = 1;
            while (p < v) p <<= 1;
            return p;
        }

        private class RingBufferFloat(int size)
        {
            private readonly float[] buf = new float[size];
            private int pos = 0;
            public int Count => buf.Length;

            public void Push(float v) { buf[pos++] = v; if (pos >= buf.Length) pos = 0; }
            public float Mean()
            {
                double s = 0;
                foreach (var x in buf) s += x;
                return (float)(s / buf.Length);
            }
            public float StdDev(float mean)
            {
                double s = 0;
                foreach (var x in buf) { double d = x - mean; s += d * d; }
                return (float)Math.Sqrt(s / buf.Length);
            }
        }
    }
}
