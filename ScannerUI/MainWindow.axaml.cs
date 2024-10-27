using Avalonia.Controls;
using Avalonia.Threading;
using SDRSharp.Radio;
using SDRSharp.RTLSDR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using ScannerUI.Audio;
using Avalonia.Media;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int RESOLUTION = 8192;

        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private double[] FrequesList { get; set; } = new double[RESOLUTION];
        private float[] DirectLine { get; set; } = new float[RESOLUTION];
        public List<DeviceDisplay> Devices { get; set; } = [];
        public RtlSdrIO? IO { get; set; } = null;
        private ConcurrentQueue<float> IntegratedSpectrumBuffer { get; set; } = [];

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

            PoweredPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            PoweredPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            PoweredPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            PoweredPlot.Plot.Title("Накопленный буфер");
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
            IntegratedSpectrumBuffer.Clear();

            Status = WorkingStatuses.STOPPED;
            ControlButton.Content = "Запуск";
        }

        private unsafe void Start()
        {
            if (IO != null)
            {
                uint freq = (uint)Frequency * 1000;

                double fSampleRate = FftSharp.FFT.FrequencyResolution(RESOLUTION, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (IO.Samplerate / 2) + (i * fSampleRate);

                float k = 1.0f / RESOLUTION;
                for (int i = 0; i < RESOLUTION; i++) DirectLine[i] = i * k;

                SignalTime = DateTime.Now;

                IO.Frequency = freq;
                IO.Start(IO_SamplesAvailable);

                ControlButton.Content = "Остановить";
                SpectrPlot.Plot.Clear();

                Status = WorkingStatuses.STARTED;
            }
        }

        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;
            Fourier.ForwardTransform(data, len);

            int gain = IO.Device?.Gain ?? 0;

            float[] power = new float[len];
            float[] simpleAveraged = new float[len];
            float[] integratedPower = new float[len];
            fixed (float* srcPower = power)
            {
                Fourier.SpectrumPower(data, srcPower, len, gain);
                fixed (float* averagedSrc = simpleAveraged)
                {
                    AudioUtils.SimpleAverage(srcPower, averagedSrc, len, 30);
                    fixed (float* cumSrc = integratedPower)
                    {
                        AudioUtils.CumulativeSum(averagedSrc, cumSrc, len);
                        AudioUtils.IntegratedSpectrum(cumSrc, len);
                    }
                }
            }

            AudioUtils.Point[] points = new AudioUtils.Point[len], filteredPoints;
            for (int i = 0; i < simpleAveraged.Length; i++) points[i] = new(FrequesList[i], simpleAveraged[i]);
            fixed (AudioUtils.Point* pointsSrc = points)
            {
                filteredPoints = RamerDouglasPeucker.Reduce(pointsSrc, 3, len);
            }

            float corellation = 1 - Math.Abs(integratedPower.Correlation(DirectLine));
            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 50)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                    SpectrPlot.Plot.Add.VerticalLine(FrequesList[RESOLUTION / 2], color: ScottPlot.Color.FromColor(System.Drawing.Color.Red));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(15 + gain, 60 + gain);
                    SpectrPlot.Refresh();

                    PoweredPlot.Plot.Clear();
                    PoweredPlot.Plot.Axes.Margins(bottom: 0);
                    PoweredPlot.Plot.Add.SignalXY(FrequesList, integratedPower);
                    PoweredPlot.Plot.Add.Line(new(FrequesList.First(), 0), new(FrequesList.Last(), 1));
                    PoweredPlot.Plot.Axes.AutoScaleX();
                    PoweredPlot.Plot.Axes.AutoScaleY();
                    PoweredPlot.Refresh();

                    CorellationBox.Text = corellation.ToString("0.0000000") + " (" + (corellation >= 0.0001f ? "Полезный сигнал" : "Шум") + ")";
                });

                SignalTime = DateTime.Now;
            }
        }
    }
}