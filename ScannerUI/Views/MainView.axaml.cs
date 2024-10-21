using Avalonia.Controls;
using Avalonia.Threading;
using ScannerUI.Audio;
using ScottPlot;
using SDRSharp.Radio;
using SDRSharp.RTLSDR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ScannerUI.Views;

public partial class MainView : UserControl
{
    public const int RESOLUTION = 8192;

    public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
    private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
    private DateTime SignalTime { get; set; } = DateTime.Now;
    private double[] FrequesList { get; set; } = [];

    private int _noiseOffset = 1;
    public List<DeviceDisplay> Devices { get; set; } = [];
    public RtlSdrIO? IO { get; set; } = null;
    private ConcurrentQueue<List<KeyValuePair<double, double>>> Buffer { get; } = [];

    public enum WorkingStatuses
    {
        NOT_INIT,
        STARTED,
        STOPPED,
    };

    public MainView()
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

        NoiseOffsetBox.ValueChanged += new((sender, args) =>
        {
            _noiseOffset = Convert.ToInt32(args.NewValue);
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

        SpectrPlot.Plot.Title("Спектр");
        SpectrPlot.Plot.XLabel("Частота (Гц)");
        SpectrPlot.Plot.YLabel("Мощность (дБ)");
        SpectrPlot.Refresh();

        PoweredPlot.Plot.Title("Накопленнй буфер");
        PoweredPlot.Plot.XLabel("Частота (Гц)");
        PoweredPlot.Plot.YLabel("Мощность (дБ)");
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
        Buffer.Clear();

        Status = WorkingStatuses.STOPPED;
        ControlButton.Content = "Запуск";
    }

    private unsafe void Start()
    {
        if (IO != null)
        {
            uint freq = (uint)Frequency * 1000;

            FrequesList = new double[RESOLUTION];
            double fSampleRate = FftSharp.FFT.FrequencyResolution(RESOLUTION, IO.Samplerate);
            for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (IO.Samplerate / 2) + (i * fSampleRate);

            SignalTime = DateTime.Now;

            IO.Frequency = freq;
            IO.Start(IO_SamplesAvailable);

            ControlButton.Content = "Остановить";
            SpectrPlot.Plot.Clear();

            Status = WorkingStatuses.STARTED;
        }
    }

    private List<KeyValuePair<double, double>> GetPoweredBuffer()
    {
        Dictionary<double, double> buffer = [];
        foreach (var listKv in Buffer)
        {
            foreach (var kv in listKv)
            {
                var freq = Math.Floor(kv.Key);
                if (buffer.TryGetValue(freq, out var val) && kv.Value > val) buffer[freq] = kv.Value;
                else buffer[freq] = kv.Value;
            }
        }

        return [.. buffer];
    }

    private unsafe void IO_SamplesAvailable(IFrontendController sender, SDRSharp.Radio.Complex* data, int len)
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
            }

            fixed (float* cumSrc = integratedPower)
            {
                AudioUtils.CumulativeSum(srcPower, cumSrc, len);
                AudioUtils.IntegratedSpectrum(cumSrc, len);
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

        var topPoints = filteredPoints.Where(p => p.Pos == AudioUtils.Point.Position.Top);
        var noiseLevel = topPoints.Select(p => p.Y).Average() + _noiseOffset;
        var positivePoints = topPoints.Where(p => p.Y - noiseLevel > _noiseOffset * 2);

        Buffer.Enqueue(positivePoints.Select(p => new KeyValuePair<double, double>(p.X, p.Y)).ToList());
        if (Buffer.Count >= 500) Buffer.TryDequeue(out var result);

        if ((DateTime.Now - SignalTime).TotalMilliseconds >= 100)
        {
            var poweredBuffer = GetPoweredBuffer();
            Dispatcher.UIThread.Post(() =>
            {
                SpectrPlot.Plot.Clear();
                SpectrPlot.Plot.Add.SignalXY(FrequesList.ToArray(), simpleAveraged);
                SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                SpectrPlot.Plot.Add.VerticalLine(FrequesList[RESOLUTION / 2], color: ScottPlot.Color.FromColor(System.Drawing.Color.Red));

                SpectrPlot.Plot.Add.HorizontalLine(noiseLevel, color: ScottPlot.Color.FromColor(System.Drawing.Color.Aqua));

                SpectrPlot.Plot.Axes.AutoScaleX();
                SpectrPlot.Plot.Axes.SetLimitsY(noiseLevel - 30, noiseLevel + 30);
                SpectrPlot.Refresh();

                PoweredPlot.Plot.Clear();
                PoweredPlot.Plot.Add.Bars(poweredBuffer.Select(p => p.Key).ToArray(), poweredBuffer.Select(p => p.Value).ToArray());
                PoweredPlot.Plot.Axes.Margins(bottom: 0);
                PoweredPlot.Plot.Axes.SetLimitsX(FrequesList.First(), FrequesList.Last());
                PoweredPlot.Refresh();
            });

            SignalTime = DateTime.Now;
        }
    }
}