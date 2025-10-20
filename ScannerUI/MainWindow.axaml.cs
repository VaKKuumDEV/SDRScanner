using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using ScannerUI.Audio;
using ScannerUI.Detector;
using ScannerUI.Helpers;
using ScottPlot;
using SDRNet.HackRfOne;
using SDRNet.Radio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 131072;
        public const int WaterfallResolution = 2_048;

        private DetectorManager DetectorManager { get; } = new();
        private BurstCollector Collector { get; }

        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private double[] FrequesList { get; set; } = new double[Resolution];
        private List<DeviceDisplay> Devices { get; set; } = [];
        private HackRFIO? IO { get; set; } = null;

        public enum WorkingStatuses { NOT_INIT, STARTED, STOPPED }
        public ConcurrentQueue<float[]> SignalQueue { get; } = new();

        public MainWindow()
        {
            DetectorManager.RegisterDetector(new WifiDetector());
            InitializeComponent();

            Unloaded += (sender, args) =>
            {
                DetectorManager.Dispose();
            };

            Collector = new(DetectorManager, result =>
            {

            });

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
            InitPlots();
        }

        private void InitPlots()
        {
            SpectrPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            DevicesListbox.Items.Clear();
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
                double fSampleRate = FftSharp.FFT.FrequencyResolution(Resolution, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (fSampleRate * Resolution / 2) + (i * fSampleRate);

                IO.Frequency = freq;
                IO.Samplerate = 2000000;

                IO.Start(IO_SamplesAvailable);
                ControlButton.Content = "Остановить";
                SpectrPlot.Plot.Clear();
                Status = WorkingStatuses.STARTED;
            }
        }

        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;

            Complex[] tempArray = new Complex[len];
            for (int i = 0; i < len; i++) tempArray[i] = data[i];
            Collector.ProcessIncoming(tempArray, IO.Samplerate, IO.Frequency);

            Fourier.ForwardTransform(data, len);
            float[] power = new float[len];
            fixed (float* pp = power)
            {
                Fourier.SpectrumPower(data, pp, len);

                float[] heatmapSlice = new float[WaterfallResolution];
                fixed (float* heatmapSliceSrc = heatmapSlice)
                {
                    AudioUtils.CompressArray(pp, len, heatmapSliceSrc, WaterfallResolution);
                }

                SignalQueue.Enqueue(heatmapSlice);
                if (SignalQueue.Count > 100) SignalQueue.TryDequeue(out _);
            }

            double[,] heatmap = new double[SignalQueue.Count, WaterfallResolution];

            for (int i = 0; i < SignalQueue.Count; i++)
            {
                if (SignalQueue.TryDequeue(out float[]? slice))
                {
                    for (int j = 0; j < WaterfallResolution; j++) heatmap[i, j] = slice[j];
                    SignalQueue.Enqueue(slice);
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                SpectrPlot.Plot.Clear();
                SpectrPlot.Plot.Add.Heatmap(heatmap);
                SpectrPlot.Plot.Axes.AutoScaleX();
                SpectrPlot.Plot.Axes.SetLimitsY(0, 100);
                SpectrPlot.Refresh();
            });
        }
    }
}
