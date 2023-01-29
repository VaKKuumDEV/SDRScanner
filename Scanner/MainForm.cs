using SDRSharp.RTLSDR;
using SDRSharp.Radio;

namespace Scanner
{
    public partial class MainForm : Form
    {
        private static RtlDevice? IO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = new();
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private List<double[]> SignalBuffer { get; } = new();
        private int SignalTime { get; set; } = 0;

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

            SpectrPlot.Plot.Title("Осциллограмма");
            SpectrPlot.Plot.XLabel("Время (мс)");
            SpectrPlot.Plot.YLabel("Амплитуда");
            SpectrPlot.Refresh();

            DevicesBox.SelectedIndexChanged += new((sender, args) =>
            {
                int index = DevicesBox.SelectedIndex;
                if(index != -1 && index < DevicesList.Count)
                {
                    if (IO != null && IO.Index == index) return;
                    else if (IO != null && IO.Index != index)
                    {
                        if(Status == WorkingStatuses.STARTED) Stop();
                        IO.Dispose();
                        IO = null;
                    }

                    DeviceDisplay device = DevicesList[index];
                    IO = new(device.Index);
                    IO.SamplesAvailable += IO_SamplesAvailable;

                    GainBox.Items.Clear();
                    foreach (int gain in IO.SupportedGains) GainBox.Items.Add(gain.ToString() + " дБ");
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
        }

        private void Stop()
        {
            Status = WorkingStatuses.STOPPED;
            IO?.Stop();
            SignalBuffer.Clear();

            AdditionalPanel.Hide();
            ControlButton.Text = "Запуск";
        }

        private void Start()
        {
            if(IO != null)
            {
                SignalTime = 0;
                uint freq = (uint)FreqBox.Value;
                int gain = 0;

                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();

                ControlButton.Text = "Остановить";
                SamplerateBox.Value = IO.Samplerate;
                SpectrPlot.Plot.Clear();
                AdditionalPanel.Show();

                Status = WorkingStatuses.STARTED;
            }
            else MessageBox.Show("Не загружено устройство", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private unsafe void IO_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
        {
            double[] tgs = new double[e.Length];
            for(int i = 0; i < e.Length; i++)
            {
                double real = e.Buffer[i].Real;
                double imag = e.Buffer[i].Imag;

                tgs[i] = 1.0 / (1.0 + Math.Pow(imag / real, 2.0));
            }

            SignalBuffer.Add(tgs);
            if(SignalBuffer.Count >= 8)
            {
                List<double> signalData = new();
                for (int i = 0; i < SignalBuffer.Count; i++) signalData.AddRange(SignalBuffer[i]);
                SignalBuffer.Clear();
                double[] times = new double[signalData.Count];
                for (int i = 0; i < times.Length; i++) times[i] = SignalTime + (i + 1);
                SignalTime += times.Length;

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddScatter(times, signalData.ToArray(), Color.Blue);
                    SpectrPlot.Refresh();
                });
            }
        }
    }
}