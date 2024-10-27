using Avalonia.Controls;
using Avalonia.Threading;
using SDRSharp.Radio;
using SDRSharp.RTLSDR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using ScannerUI.Audio;
using System.Drawing;

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
        private ConcurrentQueue<float[]> IntegratedSpectrumBuffer { get; set; } = [];
        private double _bandwidth = 0;

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        public MainWindow()
        {
            InitializeComponent();

            _bandwidth = Convert.ToDouble(BandwidthBox.Value);
            BandwidthBox.ValueChanged += new((sender, args) =>
            {
                _bandwidth = Convert.ToDouble(BandwidthBox.Value);
            });

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
            IntegratedSpectrumBuffer.Clear();

            Status = WorkingStatuses.STOPPED;
            ControlButton.Content = "Запуск";
        }

        private unsafe void Start()
        {
            if (IO != null)
            {
                uint freq = Frequency * 1000;

                double fSampleRate = FftSharp.FFT.FrequencyResolution(RESOLUTION, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (fSampleRate * RESOLUTION / 2) + (i * fSampleRate);

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

        private float[] GetTotalBuffer(int length)
        {
            var integratedBuffer = IntegratedSpectrumBuffer.ToArray();
            float[] totalBuffer = new float[length];
            for (int i = 0; i < length; i++)
            {
                float sum = 0;
                for (int j = 0; j < integratedBuffer.Length; j++)
                {
                    sum += integratedBuffer[j][i];
                }

                float average = sum / integratedBuffer.Length;
                totalBuffer[i] = average;
            }

            return totalBuffer;
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

            filteredPoints[0].Pos = AudioUtils.Point.Position.Low;
            for (int i = 1; i < filteredPoints.Length; i++)
            {
                if (filteredPoints[i].Y >= filteredPoints[i - 1].Y)
                {
                    filteredPoints[i].Pos = AudioUtils.Point.Position.Top;
                    if (filteredPoints[i - 1].Pos == AudioUtils.Point.Position.Top) filteredPoints[i - 1].Pos = AudioUtils.Point.Position.None;
                }
                else if (filteredPoints[i].Y < filteredPoints[i - 1].Y)
                {
                    filteredPoints[i].Pos = AudioUtils.Point.Position.Low;
                    if (filteredPoints[i - 1].Pos == AudioUtils.Point.Position.Low) filteredPoints[i - 1].Pos = AudioUtils.Point.Position.None;
                }
            }

            double centerFreq = IO.Frequency;
            double leftFreq = centerFreq - _bandwidth * 1000;
            double rightFreq = centerFreq + _bandwidth * 1000;

            int freqIndexMin = 0, freqIndexMax = 0;
            for (int i = 0; i < RESOLUTION; i++)
            {
                if (FrequesList[i] >= leftFreq)
                {
                    freqIndexMin = i;
                    break;
                }
            }
            for (int i = RESOLUTION - 1; i >= 0; i--)
            {
                if (FrequesList[i] <= rightFreq)
                {
                    freqIndexMax = i;
                    break;
                }
            }

            var topPoints = filteredPoints.Where(p => p.Pos == AudioUtils.Point.Position.Top);
            var noiseLevel = topPoints.Select(p => p.Y).Average();

            float corellation = 1 - Math.Abs(integratedPower.Take(new Range(freqIndexMin, freqIndexMax)).Correlation(DirectLine.Take(new Range(freqIndexMin, freqIndexMax))));
            bool isNoise = corellation < 0.0001f;
            float[]? totalBuffer = null;

            if (!isNoise) IntegratedSpectrumBuffer.Enqueue(integratedPower.Take(new Range(freqIndexMin, freqIndexMax)).ToArray());
            else if (IntegratedSpectrumBuffer.Count >= 500) IntegratedSpectrumBuffer.TryDequeue(out _);

            totalBuffer = GetTotalBuffer(freqIndexMax - freqIndexMin);
            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 500)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                    SpectrPlot.Plot.Add.VerticalLine(centerFreq, color: ScottPlot.Color.FromColor(Color.Red));
                    SpectrPlot.Plot.Add.VerticalLine(leftFreq, color: ScottPlot.Color.FromColor(Color.Green));
                    SpectrPlot.Plot.Add.VerticalLine(rightFreq, color: ScottPlot.Color.FromColor(Color.Green));

                    SpectrPlot.Plot.Add.HorizontalLine(noiseLevel, color: ScottPlot.Color.FromColor(Color.Aqua));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(noiseLevel - 30 + gain, noiseLevel + 30 + gain);
                    SpectrPlot.Refresh();

                    PoweredPlot.Plot.Clear();
                    PoweredPlot.Plot.Add.Signal(integratedPower.Take(new Range(freqIndexMin, freqIndexMax)).ToArray(), color: ScottPlot.Color.FromColor(Color.Blue));
                    //PoweredPlot.Plot.Add.Line(new(0, 0), new(freqIndexMax - freqIndexMin, 1)).Color = ScottPlot.Color.FromColor(Color.Orange);
                    //PoweredPlot.Plot.Axes.SetLimits(0, freqIndexMax, 0, 1);
                    PoweredPlot.Plot.Axes.AutoScale();
                    PoweredPlot.Refresh();

                    CorellationBox.Text = corellation.ToString("0.0000000") + " (" + (isNoise ? "Шум" : "Полезный сигнал") + ")";
                });

                SignalTime = DateTime.Now;
            }
        }
    }
}