using Avalonia.Controls;
using Avalonia.Threading;
using SDRNet.Radio;
using SDRNet.HackRfOne;
using System.Collections.Generic;
using System;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 131072;

        public uint Frequency { get => Convert.ToUInt32(FrequencyBox.Value); set => FrequencyBox.Value = Convert.ToDecimal(value); }
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private double[] FrequesList { get; set; } = new double[Resolution];
        public List<DeviceDisplay> Devices { get; set; } = [];
        public HackRFIO? IO { get; set; } = null;

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

                    GainsBox.Items.Clear();
                    if (IO.Device != null)
                    {
                        //foreach (int gain in IO.Device.SupportedGains) GainsBox.Items.Add(gain.ToString() + " дБ");
                        if (GainsBox.Items.Count > 0) GainsBox.SelectedIndex = 0;
                    }

                    Status = WorkingStatuses.STOPPED;
                }
            });

            GainsBox.SelectionChanged += new((sender, args) =>
            {
                /*int index = GainsBox.SelectedIndex;
                if (index != -1 && IO != null && IO.Device != null && index < IO.Device.SupportedGains.Length)
                {
                    IO.Device.Gain = IO.Device.SupportedGains[index];
                }*/
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

                double fSampleRate = FftSharp.FFT.FrequencyResolution(Resolution, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (fSampleRate * Resolution / 2) + (i * fSampleRate);

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

            float[] power = new float[len];
            fixed (float* p = power)
            {
                Fourier.SpectrumPower(data, p, len);
            }

            Dispatcher.UIThread.Post(() =>
            {
                SpectrPlot.Plot.Clear();
                SpectrPlot.Plot.Add.SignalXY(FrequesList, power);

                SpectrPlot.Plot.Axes.AutoScaleX();
                SpectrPlot.Plot.Axes.AutoScaleY();
                SpectrPlot.Refresh();
            });
        }
    }
}