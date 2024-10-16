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
        private double[] FrequesList { get; set; } = [];
        private int Bandwidth { get => Convert.ToInt32(bandwidthBox.Value); set => bandwidthBox.Value = value; }

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
            InitializeComponent();

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

            ControlButton.Text = "Запуск";
        }

        private void Start()
        {
            if (IO != null)
            {
                uint freq = (uint)FreqBox.Value * 1000;
                int gain = 0;

                FrequesList = new double[RESOLUTION];
                double fSampleRate = FFT.FrequencyResolution(RESOLUTION, IO.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (IO.Samplerate / 2) + (i * fSampleRate);

                SignalTime = DateTime.Now;
                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();
                IO.UseTunerAGC = true;

                ControlButton.Text = "Остановить";
                SpectrPlot.Plot.Clear();

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
            float[] integratedPower = new float[e.Length];
            fixed (float* srcPower = power)
            {
                Fourier.SpectrumPower(e.Buffer, srcPower, e.Length, IO.Gain);
                fixed (float* averagedSrc = simpleAveraged)
                {
                    AudioUtils.SimpleAverage(srcPower, averagedSrc, e.Length, 30);
                    fixed (float* cumSrc = integratedPower)
                    {
                        AudioUtils.CumulativeSum(averagedSrc, cumSrc, e.Length);
                        AudioUtils.IntegratedSpectrum(cumSrc, e.Length);
                    }
                }
            }

            AudioUtils.Point[] points = new AudioUtils.Point[e.Length], filteredPoints;
            for (int i = 0; i < simpleAveraged.Length; i++) points[i] = new(FrequesList[i], simpleAveraged[i]);
            fixed (AudioUtils.Point* pointsSrc = points)
            {
                filteredPoints = RamerDouglasPeucker.Reduce(pointsSrc, 3, e.Length);
            }

            AudioUtils.Point[] integratedSpectrumPoints = new AudioUtils.Point[e.Length], filteredIntegratedSpectrumPoints;
            for (int i = 0; i < integratedPower.Length; i++) integratedSpectrumPoints[i] = new(FrequesList[i], integratedPower[i]);
            fixed (AudioUtils.Point* pointsSrc = integratedSpectrumPoints)
            {
                filteredIntegratedSpectrumPoints = RamerDouglasPeucker.Reduce(pointsSrc, 0.1d, e.Length);
            }

            double centerFreq = FrequesList[RESOLUTION / 2];

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 100)
            {
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

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(FrequesList.ToArray(), simpleAveraged);
                    SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                    SpectrPlot.Plot.Add.VerticalLine(centerFreq, color: ScottPlot.Color.FromColor(Color.Red));

                    //foreach (var point in filteredPoints.Where(p => p.IsLow)) SpectrPlot.Plot.Add.VerticalLine(point.X, color: ScottPlot.Color.FromColor(Color.Orange));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(20, 60);
                    SpectrPlot.Refresh();

                    NoisePlot.Plot.Clear();
                    NoisePlot.Plot.Add.SignalXY(FrequesList, integratedPower);
                    NoisePlot.Plot.Add.Line(new(FrequesList.First(), 0), new(FrequesList.Last(), 1));
                    NoisePlot.Plot.Axes.AutoScaleX();
                    NoisePlot.Plot.Axes.AutoScaleY();
                    NoisePlot.Refresh();
                });
                SignalTime = DateTime.Now;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Status == WorkingStatuses.STARTED) Stop();
        }
    }
}