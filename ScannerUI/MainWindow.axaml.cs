using Avalonia.Controls;
using Avalonia.Threading;
using SDRSharp.Radio;
using SDRSharp.RTLSDR;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 8192;
        private const int AverageCount = 50; // Количество усреднений

        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private double[] FrequesList { get; set; } = new double[Resolution];
        public List<DeviceDisplay> Devices { get; set; } = [];
        public RtlSdrIO? IO { get; set; } = null;

        private readonly Lock _lock = new();
        private ConcurrentQueue<float[]> SpectrumBuffer = [];
        private float AverageNoiseLevel = 0;

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
                    IO = new RtlSdrIO();
                    IO.SelectDevice(device.Index);

                    GainsBox.Items.Clear();
                    if (IO.Device != null)
                    {
                        foreach (int gain in IO.Device.SupportedGains) GainsBox.Items.Add(gain.ToString() + " дБ");
                        if (GainsBox.Items.Count > 0) GainsBox.SelectedIndex = 0;
                    }

                    Status = WorkingStatuses.STOPPED;
                }
            });

            GainsBox.SelectionChanged += new((sender, args) =>
            {
                int index = GainsBox.SelectedIndex;
                if (index != -1 && IO != null && IO.Device != null && index < IO.Device.SupportedGains.Length)
                {
                    IO.Device.Gain = IO.Device.SupportedGains[index];
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
            SpectrumBuffer = [];

            Status = WorkingStatuses.STOPPED;
            ControlButton.Content = "Запуск";
        }

        private unsafe void Start()
        {
            if (IO != null)
            {
                SpectrumBuffer = [];
                uint freq = Frequency * 1000;

                double fSampleRate = FftSharp.FFT.FrequencyResolution(Resolution, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (fSampleRate * Resolution / 2) + (i * fSampleRate);

                SignalTime = DateTime.Now;

                IO.Frequency = freq;
                IO.Start(IO_SamplesAvailable);

                ControlButton.Content = "Остановить";
                SpectrPlot.Plot.Clear();

                Status = WorkingStatuses.STARTED;
            }
        }

        private float[] GetSummarizedBuffer()
        {
            var buffer = SpectrumBuffer.ToArray();
            var resultBuffer = new float[Resolution];

            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < buffer.Length; j++)
                {
                    resultBuffer[i] += buffer[j][i];
                }
            }

            for (int i = 0; i < Resolution; i++)
            {
                resultBuffer[i] /= buffer.Length;
            }

            return resultBuffer;
        }

        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;
            Fourier.ForwardTransform(data, len);

            int gain = IO.Device?.Gain ?? 0;

            lock (_lock)
            {
                float[] dataFloat = new float[len];
                for (int i = 0; i < len; i++)
                {
                    dataFloat[i] = data[i].ModulusSquared();
                }
                SpectrumBuffer.Enqueue(dataFloat);

                if (SpectrumBuffer.Count >= AverageCount)
                {
                    float noiseFloorDb = CalculateNoiseFloor(GetSummarizedBuffer()) + gain;

                    AverageNoiseLevel = noiseFloorDb;
                    SpectrumBuffer.TryDequeue(out var dequeueResult);
                }
            }

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 50)
            {
                var power = GetSummarizedBuffer();
                fixed (float* powerSrc = power)
                {
                    for (int i = 0; i < len; i++)
                    {
                        powerSrc[i] = (float)(10 * Math.Log10(powerSrc[i] + float.Epsilon)) + gain;
                    }
                }

                Dispatcher.UIThread.Post(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.Signal(power);
                    SpectrPlot.Plot.Add.HorizontalLine(AverageNoiseLevel);

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(AverageNoiseLevel - 40, AverageNoiseLevel + 40);
                    SpectrPlot.Refresh();
                });

                SignalTime = DateTime.Now;
            }
        }

        private static float CalculateNoiseFloor(float[] spectrum)
        {
            // Применение медианного фильтра для сглаживания спектра
            float[] smoothedSpectrum = ApplyMedianFilter(spectrum, 5);
            // float[] smoothedSpectrum = spectrum;

            // Кумулятивная сумма
            float[] cdf = new float[smoothedSpectrum.Length];
            cdf[0] = smoothedSpectrum[0];
            for (int i = 1; i < smoothedSpectrum.Length; i++)
                cdf[i] = cdf[i - 1] + smoothedSpectrum[i];

            // Поиск точки максимального отклонения
            double maxDeviation = 0;
            int thresholdIndex = 0;

            for (int i = 0; i < cdf.Length; i++)
            {
                double ideal = (double)(i + 1) / cdf.Length;
                double deviation = Math.Abs(cdf[i] / cdf.Last() - ideal);

                if (deviation > maxDeviation)
                {
                    maxDeviation = deviation;
                    thresholdIndex = i;
                }
            }

            // Расчет мощности шума
            float noisePower = cdf[thresholdIndex] / (thresholdIndex + 1);
            return (float)(10 * Math.Log10(noisePower + float.Epsilon)) + 4.5f;
        }

        private static float[] ApplyMedianFilter(float[] data, int windowSize)
        {
            float[] result = new float[data.Length];
            int halfWindow = windowSize / 2;

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - halfWindow);
                int end = Math.Min(data.Length - 1, i + halfWindow);
                float[] window = new float[end - start + 1];

                for (int j = start; j <= end; j++)
                {
                    window[j - start] = data[j];
                }

                Array.Sort(window);
                result[i] = window[window.Length / 2];
            }

            return result;
        }
    }
}