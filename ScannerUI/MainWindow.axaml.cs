using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using ScannerUI.Audio;
using ScannerUI.Detector;
using ScannerUI.Helpers;
using SDRNet.HackRfOne;
using SDRNet.Radio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int ScanInterval = 2;
        public const int WaterfallResolution = 2_048;
        public const int WaterfallHeight = 300;

        private DetectorManager DetectorManager { get; } = new();
        private BurstCollector Collector { get; }

        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private List<DeviceDisplay> Devices { get; set; } = [];
        private IFrontendController? IO { get; set; } = null;

        public enum WorkingStatuses { NOT_INIT, STARTED, STOPPED }
        public ConcurrentQueue<float[]> SignalQueue { get; } = new();

        private Thread? WorkerThread { get; set; } = null;
        private CancellationTokenSource Cts { get; set; } = new();

        private int[] ScanFreqs { get; set; } = [];
        private int CurrentFreqIndex { get; set; } = 0;

        private ObservableCollection<DetectionResult> DetectedDevices { get; } = [];

        public unsafe MainWindow()
        {
            DetectorManager.RegisterDetector(new WifiDetector());
            DetectorManager.RegisterDetector(new SmartDeviceDetector());
            InitializeComponent();

            Unloaded += (sender, args) =>
            {
                Stop();

                Cts.Dispose();
                DetectorManager.Dispose();
            };

            Collector = new(DetectorManager, result =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var res in result)
                    {
                        if (!DetectedDevices.Contains(res))
                        {
                            DetectedDevices.Add(res);
                        }
                    }
                });
            });

            DevicesBox.SelectionChanged += new((sender, args) =>
            {
                int index = DevicesBox.SelectedIndex;
                if (index != -1 && index < Devices.Count)
                {
                    DeviceDisplay device = Devices[index];
                    IO = new HackRFIO();
                    ((HackRFIO)IO).SelectDevice(device.Index);
                    IO.Samplerate = 2000000;
                    IO.SamplesAvailable += IO_SamplesAvailable;
                    Status = WorkingStatuses.STOPPED;
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });

            DevicesListbox.ItemsSource = DetectedDevices;

            LoadDevicesList();
            InitPlots();
        }

        private unsafe void ProcessLoop()
        {
            try
            {
                while (!Cts.IsCancellationRequested && IO != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        StatusLabel.Content = "Сканирую на частоте " + ScanFreqs[CurrentFreqIndex] + " МГц";
                    });

                    IO.Frequency = (long)(ScanFreqs[CurrentFreqIndex] * 1e6);

                    IO.Start();

                    Thread.Sleep(ScanInterval * 1000);

                    CurrentFreqIndex++;
                    if (CurrentFreqIndex >= ScanFreqs.Length) CurrentFreqIndex = 0;

                    IO.Stop();
                    if (ScanFreqs.Length == 0) break;
                }
            }
            catch (ThreadAbortException)
            {
                // Игнорируем завершение
            }
        }

        private static int[] GetScanFreqs(bool wifi2g, bool wifi5g, bool smartDevices)
        {
            List<int> freqs = [];
            if (wifi2g)
            {
                for (int i = 1; i <= 14; i++)
                {
                    int freq = Convert.ToInt32(WiFiChannelHelper.ChannelToFreqMHz(i));
                    freqs.Add(freq);
                }
            }

            if (wifi5g)
            {
                for (int i = 36; i <= 165; i++)
                {
                    int freq = Convert.ToInt32(WiFiChannelHelper.ChannelToFreqMHz(i));
                    freqs.Add(freq);
                }
            }

            if (smartDevices)
            {
                freqs.Add(433);
            }

            return [.. freqs.Order()];
        }

        private void InitPlots()
        {
            WaterfallPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            WaterfallPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            WaterfallPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            WaterfallPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            WaterfallPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            WaterfallPlot.Plot.Title("Водопад");
            WaterfallPlot.Refresh();

            SpectrPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            CorellationPlot.Plot.Title("Отклонение от прямой");
            CorellationPlot.Plot.Axes.Frameless();
            CorellationPlot.Refresh();
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

        private async void Stop()
        {
            ControlButton.Content = "Останавливаю...";
            ControlButton.IsEnabled = false;

            Cts.Cancel();
            await Task.Run(() => WorkerThread?.Join(ScanInterval * 1000));

            DetectedDevices.Clear();
            Status = WorkingStatuses.STOPPED;
            ControlButton.Content = "Запуск";
            ControlButton.IsEnabled = true;

            StatusLabel.Content = "Сканирование не ведется";
            DeviationLabel.Content = "Сканирование не ведется";
        }

        private unsafe void Start()
        {
            if (IO != null)
            {
                var scanFreqs = GetScanFreqs(Wifi2GCheckbox.IsChecked ?? false, Wifi5GCheckbox.IsChecked ?? false, SmartDevicesCheckbox.IsChecked ?? false);

                if (scanFreqs.Length > 0)
                {
                    WorkerThread = new Thread(ProcessLoop)
                    {
                        IsBackground = true,
                        Name = "MainWindowWorker"
                    };
                    WorkerThread.Start();

                    StatusLabel.Content = "Запускаю сканирование...";
                    ScanFreqs = scanFreqs;
                    CurrentFreqIndex = 0;
                    Cts = new();

                    ControlButton.Content = "Остановить";
                    SpectrPlot.Plot.Clear();
                    Status = WorkingStatuses.STARTED;
                }
            }
        }

        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;

            double fSampleRate = FftSharp.FFT.FrequencyResolution(len, sender.Samplerate);
            var freqsZero = FftSharp.FFT.FrequencyScale(len, sender.Samplerate);
            var freqs = new double[len];
            var startFreq = sender.Frequency - (fSampleRate * len / 2);
            for (int i = 0; i < len; i++)
            {
                freqs[i] = startFreq + (i * fSampleRate);
            }


            Fourier.ForwardTransform(data, len);
            float[] power = new float[len];
            float[] integratedSpectrum = new float[len];
            double mae;
            float noiseLevel;
            fixed (float* pp = power)
            {
                Fourier.SpectrumPower(data, pp, len);
                noiseLevel = RobustNoiseEstimator.EstimateNoiseLevel(pp, len, 1024, 1) + 12f;

                fixed (float* integratedSpectrumSrc = integratedSpectrum)
                {
                    AudioUtils.CumulativeSum(pp, integratedSpectrumSrc, len);
                    AudioUtils.IntegratedSpectrum(integratedSpectrumSrc, len);

                    mae = AudioUtils.MeanAbsoluteError(integratedSpectrumSrc, freqsZero, len);
                }

                float[] heatmapSlice = new float[WaterfallResolution];
                fixed (float* heatmapSliceSrc = heatmapSlice)
                {
                    AudioUtils.CompressArray(pp, len, heatmapSliceSrc, WaterfallResolution);
                }

                SignalQueue.Enqueue(heatmapSlice);
                if (SignalQueue.Count > WaterfallHeight) SignalQueue.TryDequeue(out _);
            }
            Collector.ProcessIncoming(power, noiseLevel, IO.Samplerate, IO.Frequency);

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
                DeviationLabel.Content = "Отклонение от белого шума: " + mae;

                SpectrPlot.Plot.Clear();
                SpectrPlot.Plot.Add.HorizontalLine(noiseLevel, color: ScottPlot.Colors.Red);
                SpectrPlot.Plot.Add.SignalXY(freqs, power, color: ScottPlot.Colors.Blue);
                SpectrPlot.Plot.Axes.AutoScaleX();
                SpectrPlot.Plot.Axes.SetLimitsY(-20, 80);
                SpectrPlot.Refresh();

                WaterfallPlot.Plot.Clear();
                var hm = WaterfallPlot.Plot.Add.Heatmap(heatmap);
                hm.Colormap = new ScottPlot.Colormaps.Turbo();
                WaterfallPlot.Plot.Axes.AutoScaleX();
                WaterfallPlot.Plot.Axes.SetLimitsY(0, WaterfallHeight);
                WaterfallPlot.Refresh();

                CorellationPlot.Plot.Clear();
                CorellationPlot.Plot.Add.Line(new(0, 0, len - 1, 1));
                CorellationPlot.Plot.Add.Signal(integratedSpectrum);
                CorellationPlot.Plot.Axes.AutoScale();
                CorellationPlot.Refresh();
            });
        }
    }
}
