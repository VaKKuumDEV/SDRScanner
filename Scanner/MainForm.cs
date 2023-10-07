using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using System.Numerics;

namespace Scanner
{
    public partial class MainForm : Form
    {
        private static RtlDevice? IO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = new();
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private StreamPlayer Player { get; set; }

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        public MainForm()
        {
            InitializeComponent();
            AdditionalPanel.Hide();

            SpectrPlot.Plot.Title("�������������");
            SpectrPlot.Plot.XLabel("����� (��)");
            SpectrPlot.Plot.YLabel("���������");
            SpectrPlot.Refresh();

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

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });

            Player = new();
            Player.PlayAsync();
            Random rnd = new();

            Thread thread = new(new ThreadStart(() =>
            {
                while (!IsDisposed)
                {
                    var freq = 0.01 * rnd.Next(10);
                    for (int i = 0; i < 1000; i++)
                    {
                        var v = (short)(Math.Sin(freq * i * Math.PI * 2) * short.MaxValue);
                        Player.Write(v);
                    }
                }
            }));
            thread.Start();

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
        }

        private void Stop()
        {
            Status = WorkingStatuses.STOPPED;
            IO?.Stop();

            AdditionalPanel.Hide();
            ControlButton.Text = "������";
        }

        private void Start()
        {
            if (IO != null)
            {
                SignalTime = DateTime.Now;
                uint freq = (uint)FreqBox.Value;
                int gain = 0;

                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();

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
            float* power = stackalloc float[e.Length];
            Fourier.SpectrumPower(e.Buffer, power, e.Length);

            if ((DateTime.Now - SignalTime).TotalSeconds >= 1)
            {
                List<double> fftPower = new();
                for (int i = 0; i < e.Length; i++) fftPower.Add(power[i]);
                double averagePower = fftPower.Average();

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddSignal(fftPower.ToArray());
                    SpectrPlot.Plot.AddHorizontalLine(averagePower, Color.Green);
                    SpectrPlot.Refresh();

                    AveragePowerBox.Text = averagePower.ToString();
                });
                SignalTime = DateTime.Now;
            }
        }
    }
}