using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using SDRSharp.Radio.PortAudio;
using FftSharp;

namespace Scanner
{
    public partial class MainForm : Form
    {
        public const int RESOLUTION = 8192;
        public const int BUFFER_LENGTH = 50;
        public const double KORREL_DOPUSK = 0.5;

        private static RtlDevice? IO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = [];
        private List<AudioDevice> AudioDevices { get; } = [];
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private List<double> FrequesList { get; set; } = [];
        private List<string> AudioBuffer { get; set; } = [];
        private int Iter { get; set; } = 0;
        private SignalsMap Map { get; }
        private string? RecordingSignal { get; set; } = null;
        private List<string> RecordingBuffer { get; set; } = [];
        public double NoiseLevel { get => Convert.ToDouble(NoiseLevelBox.Value); }

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        public struct BandwidthInfo
        {
            public int Samplerate { get; set; }
            public int FromIndex { get; set; }
            public int ToIndex { get; set; }
            public double Left { get; set; }
            public double Right { get; set; }
        };

        unsafe public MainForm()
        {
            Map = new(new DirectoryInfo(Environment.CurrentDirectory + "/Samples.json").FullName);
            InitializeComponent();
            AdditionalPanel.Hide();
            StartSignalRecButton.Hide();

            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
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
                    foreach (int gain in IO.SupportedGains) GainBox.Items.Add(gain.ToString() + " дБ");
                    if (GainBox.Items.Count > 0) GainBox.SelectedIndex = 0;
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

            StartSignalRecButton.Hide();
            AdditionalPanel.Hide();
            ControlButton.Text = "Запуск";
        }

        private void Start()
        {
            if (IO != null)
            {
                AudioBuffer.Clear();

                uint freq = (uint)FreqBox.Value * 1000;
                int gain = 0;

                FrequesList = new(new double[RESOLUTION]);
                double fSampleRate = FFT.FrequencyResolution(RESOLUTION, IO.Samplerate);
                for (int i = 0; i < FrequesList.Count; i++) FrequesList[i] = freq - (IO.Samplerate / 2) + (i * fSampleRate);

                SignalTime = DateTime.Now;
                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();
                IO.UseTunerAGC = true;

                ControlButton.Text = "Остановить";
                SamplerateBox.Value = IO.Samplerate;
                SpectrPlot.Plot.Clear();
                AdditionalPanel.Show();
                StartSignalRecButton.Show();

                Status = WorkingStatuses.STARTED;
            }
            else MessageBox.Show("Не загружено устройство", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private unsafe void IO_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
        {
            if (IO == null) return;
            Fourier.ForwardTransform(e.Buffer, e.Length);

            float[] power = new float[e.Length];
            float[] simpleAveraged = new float[e.Length];
            fixed (float* srcPower = power)
            {
                Fourier.SpectrumPower(e.Buffer, srcPower, e.Length, IO.Gain);
                fixed (float* averagedSrc = simpleAveraged) AudioUtils.SimpleAverage(srcPower, averagedSrc, e.Length, 30);
            }

            bool isPositiveSignal = false;
            float average = simpleAveraged.Average();

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 500)
            {
                List<AudioUtils.Point> points = [];
                for (int i = 0; i < simpleAveraged.Length; i++) points.Add(new(FrequesList[i], simpleAveraged[i]));
                var filteredPoints = RamerDouglasPeucker.Reduce([.. points], 3);

                int offset = 0;
                bool isLow = true;
                while (offset < filteredPoints.Length - 1)
                {
                    if (isLow)
                    {
                        int index = AudioUtils.GetBottomPoint(filteredPoints.Skip(offset).ToList());
                        for (int j = offset; j < offset + index; j++) filteredPoints[j].IsLow = false;
                        offset += index;
                        isLow = false;
                    }
                    else
                    {
                        int index = AudioUtils.GetTopPoint(filteredPoints.Skip(offset).ToList());
                        for (int j = offset; j < offset + index; j++) filteredPoints[j].IsLow = true;
                        offset += index;
                        isLow = true;
                    }
                }

                List<AudioUtils.Point> highPoints = [];
                for (int i = 0; i < filteredPoints.Length; i++) if (!filteredPoints[i].IsLow && (filteredPoints[i].Y - average >= NoiseLevel)) highPoints.Add(filteredPoints[i]);
                if (highPoints.Count > 5) isPositiveSignal = true;

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    //SpectrPlot.Plot.Add.SignalXY(FrequesList.ToArray(), power);
                    SpectrPlot.Plot.Add.SignalXY(FrequesList.ToArray(), simpleAveraged);
                    SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                    SpectrPlot.Plot.Add.VerticalLine(FrequesList[FrequesList.Count / 2], color: ScottPlot.Color.FromColor(Color.Red));

                    SpectrPlot.Plot.Add.HorizontalLine(average, color: ScottPlot.Color.FromColor(Color.Chocolate));
                    SpectrPlot.Plot.Add.HorizontalLine(average + NoiseLevel, color: ScottPlot.Color.FromColor(Color.LightBlue));
                    SpectrPlot.Plot.Add.HorizontalLine(average - NoiseLevel, color: ScottPlot.Color.FromColor(Color.LightBlue));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(20, 60);
                    SpectrPlot.Refresh();

                    HashBox.Text = isPositiveSignal ? "Полезный" : "Шум";
                    SignalNameBox.Text = highPoints.Count.ToString();
                });
                SignalTime = DateTime.Now;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Status == WorkingStatuses.STARTED) Stop();
        }

        private void DatabaseButton_Click(object sender, EventArgs e)
        {
            SamplerForm form = new();
            form.ShowDialog(this);

            Map.Reload();
        }

        private void StartSignalRecButton_Click(object sender, EventArgs e)
        {
            if (RecordingSignal == null)
            {
                SelectSignalForm form = new();
                form.ShowDialog(this);

                if (form.SelectedSignal != null)
                {
                    Map.Reload();
                    RecordingBuffer.Clear();
                    RecordingSignal = form.SelectedSignal;
                    StartSignalRecButton.Text = "Остановить запись";
                }
            }
            else
            {
                if (RecordingSignal != null)
                {
                    Map.Reload();
                    if (!Map.Map.ContainsKey(RecordingSignal)) Map.Map[RecordingSignal] = [];
                    Map.Map[RecordingSignal].AddRange(RecordingBuffer);
                    Map.Save();
                }

                StartSignalRecButton.Text = "Записать в базу";
                RecordingSignal = null;
                RecordingBuffer.Clear();
            }
        }
    }
}