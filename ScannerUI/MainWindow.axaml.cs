using Avalonia.Controls;
using Avalonia.Threading;
using SDRNet.Radio;
using SDRNet.HackRfOne;
using System;
using System.Collections.Generic;
using ScannerUI.Detectors;
using ScannerUI.Helpers;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 131072;

        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private double[] FrequesList { get; set; } = new double[Resolution];
        private List<DeviceDisplay> Devices { get; set; } = [];
        private HackRFIO? IO { get; set; } = null;

        // components
        private readonly BurstCollector burstCollector;
        private readonly DetectorManager detectorManager;

        public enum WorkingStatuses { NOT_INIT, STARTED, STOPPED }

        public MainWindow()
        {
            InitializeComponent();

            // init components
            detectorManager = new DetectorManager();
            detectorManager.RegisterDetector(new Detector433()); // only 433 MHz detector enabled for now

            burstCollector = new BurstCollector(onBurstReady: (burst, samplerate) =>
            {
                if (detectorManager.ProcessBurst(burst, samplerate) is { } newDetectedDevice)
                {
                    DevicesListbox.Items.Add(newDetectedDevice);
                }
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
                burstCollector.Dispose();
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

                // configure burstCollector with samplerate (silence threshold depends on samplerate)
                burstCollector.ConfigureForSampleRate(IO.Samplerate);

                IO.Start(IO_SamplesAvailable);
                ControlButton.Content = "Остановить";
                SpectrPlot.Plot.Clear();
                Status = WorkingStatuses.STARTED;
            }
        }

        // core callback from hackrf -> keep minimal and non-blocking
        private unsafe void IO_SamplesAvailable(IFrontendController sender, Complex* data, int len)
        {
            if (IO == null) return;

            // copy IQ to managed array quickly
            Complex[] iq = new Complex[len];
            for (int i = 0; i < len; i++) iq[i] = data[i];

            // pass to burstCollector (which updates noise model and emits bursts)
            burstCollector.ProcessIncoming(iq);

            // prepare spectrum for UI only (non-destructive)
            // we reuse the existing Fourier calls like before
            fixed (Complex* p = iq)
            {
                Complex* tmp = stackalloc Complex[len];
                for (int i = 0; i < len; i++) tmp[i] = p[i];

                Fourier.ForwardTransform(tmp, len);
                float[] power = new float[len];
                fixed (float* pp = power)
                {
                    Fourier.SpectrumPower(tmp, pp, len);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(FrequesList, power);
                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(10, 80);
                    SpectrPlot.Refresh();

                    CorellationBox.Text = $"Уникальных устройств: {detectorManager.TotalUniqueDevices}";
                });
            }
        }
    }
}
