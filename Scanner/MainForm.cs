using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using System.Numerics;
using System.Media;
using SDRSharp.Radio.PortAudio;
using FftSharp;

namespace Scanner
{
    public partial class MainForm : Form
    {
        private static RtlDevice? IO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = new();
        private List<AudioDevice> AudioDevices { get; } = new();
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private List<double> FrequesList { get; set; } = new();
        private Queue<double> AudioBuffer { get; set; } = new();
        private StreamPlayer? Player { get; set; } = null;
        private BandwidthInfo? Bandwidth { get; set; } = null;

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        public struct BandwidthInfo
        {
            public int FromIndex { get; set; }
            public int ToIndex { get; set; }
            public double Left { get; set; }
            public double Right { get; set; }
        };

        unsafe public MainForm()
        {
            InitializeComponent();
            AdditionalPanel.Hide();

            SpectrPlot.Plot.Title("������");
            SpectrPlot.Plot.XLabel("������� (��)");
            SpectrPlot.Plot.YLabel("�������� (��)");
            SpectrPlot.Refresh();

            SignalPlot.Plot.Title("������");
            SignalPlot.Plot.XLabel("����� (��)");
            SignalPlot.Plot.YLabel("���������");
            SignalPlot.Refresh();

            DevicesBox.SelectedIndexChanged += new((sender, args) =>
            {
                int index = DevicesBox.SelectedIndex;
                if (index != -1 && index < DevicesList.Count)
                {
                    if (IO != null && IO.Index == index) return;
                    else if (IO != null && IO.Index != index)
                    {
                        if (Status == WorkingStatuses.STARTED) Stop();
                        IO.Dispose();
                        IO = null;
                    }

                    DeviceDisplay device = DevicesList[index];
                    IO = new(device.Index);
                    IO.SamplesAvailable += IO_SamplesAvailable;

                    GainBox.Items.Clear();
                    foreach (int gain in IO.SupportedGains) GainBox.Items.Add(gain.ToString() + " ��");
                }
            });

            FreqBox.ValueChanged += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED)
                {
                    IO?.Stop();
                    IO?.Start();
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });

            LoadDevicesList();
        }

        private void LoadDevicesList()
        {
            DeviceDisplay[] devices = DeviceDisplay.GetActiveDevices();
            DevicesList.Clear();
            DevicesList.AddRange(devices);

            DevicesBox.Items.Clear();
            foreach (DeviceDisplay device in DevicesList) DevicesBox.Items.Add("(#" + device.Index + ") " + device.Name);

            Status = WorkingStatuses.STOPPED;
            if (DevicesList.Count > 0) DevicesBox.SelectedIndex = 0;
        }

        private void Stop()
        {
            Status = WorkingStatuses.STOPPED;
            IO?.Stop();
            Player?.Stop();

            AdditionalPanel.Hide();
            ControlButton.Text = "������";
        }

        private void Start()
        {
            if (IO != null)
            {
                AudioBuffer.Clear();
                Player = new(8192);
                Player.PlayAsync();

                uint freq = (uint)FreqBox.Value * 1000;
                int gain = 0;

                FrequesList = new(new double[8192]);
                double fSampleRate = Convert.ToInt32(IO.Samplerate) / FrequesList.Count;
                for (int i = 0; i < FrequesList.Count; i++) FrequesList[i] = freq - (IO.Samplerate / 2) + (i * fSampleRate);

                int bandwidth = 100000;
                int bandwidthIndexes = (int)Math.Floor(bandwidth / 2 / fSampleRate);
                Bandwidth = new()
                {
                    FromIndex = FrequesList.Count / 2 - bandwidthIndexes,
                    ToIndex = FrequesList.Count / 2 + bandwidthIndexes,
                    Left = FrequesList[FrequesList.Count / 2] - (bandwidth / 2),
                    Right = FrequesList[FrequesList.Count / 2] + (bandwidth / 2),
                };

                SignalTime = DateTime.Now;
                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();
                IO.UseTunerAGC = true;

                ControlButton.Text = "����������";
                SamplerateBox.Value = IO.Samplerate;
                SpectrPlot.Plot.Clear();
                AdditionalPanel.Show();

                Status = WorkingStatuses.STARTED;
            }
            else MessageBox.Show("�� ��������� ����������", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private unsafe void IO_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
        {
            //Fourier.InverseTransform(e.Buffer, e.Length);
            Fourier.ForwardTransform(e.Buffer, e.Length, true);

            float[] window = FilterBuilder.MakeWindow(WindowType.Hamming, e.Length);
            fixed (float* src = window) Fourier.ApplyFFTWindow(e.Buffer, src, e.Length);

            float[] audio = new float[e.Length];
            fixed (float* src = audio) new AmDetector().Demodulate(e.Buffer, src, e.Length);

            float[] power = new float[e.Length];
            fixed (float* src = power) Fourier.SpectrumPower(e.Buffer, src, e.Length);

            for (int i = Bandwidth.Value.FromIndex; i < Bandwidth.Value.ToIndex; i++)
            {
                var sh = (short)(power[i] * short.MaxValue);
                Player.Write(sh);
            }

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 500)
            {
                List<double> fftPower = new(new double[e.Length]);
                for (int i = 0; i < e.Length; i++) fftPower[i] = power[i];

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddSignalXY(FrequesList.ToArray(), fftPower.ToArray());
                    SpectrPlot.Plot.AddVerticalLine(FrequesList[FrequesList.Count / 2], Color.Red);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Left, Color.Green);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Right, Color.Green);
                    SpectrPlot.Plot.SetAxisLimitsY(-20, 100);
                    SpectrPlot.Refresh();

                    SignalPlot.Plot.Clear();
                    SignalPlot.Plot.AddSignal(audio);
                    SignalPlot.Plot.SetAxisLimitsY(0, 1000);
                    SignalPlot.Refresh();

                    AveragePowerBox.Text = "NaN";
                });
                SignalTime = DateTime.Now;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }
    }
}