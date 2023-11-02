using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using System.Numerics;
using System.Media;
using SDRSharp.Radio.PortAudio;
using FftSharp;
using System.Linq;
using NAudio.Wave;
using static Scanner.NeuroForm;
using System.Diagnostics.Metrics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        private List<string> AudioBuffer { get; set; } = new();
        private StreamPlayer? Player { get; set; } = null;
        private BandwidthInfo? Bandwidth { get; set; } = null;
        private int Iter { get; set; } = 0;
        private SignalsMap Map { get; }

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

            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            SignalPlot.Plot.Title("Сигнал");
            SignalPlot.Plot.XLabel("Частота (Гц)");
            SignalPlot.Plot.YLabel("Амплитуда");
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
                    foreach (int gain in IO.SupportedGains) GainBox.Items.Add(gain.ToString() + " дБ");
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
            ControlButton.Text = "Запуск";
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

                int bandwidth = (int)BandwidthBox.Value;
                int bandwidthIndexes = (int)Math.Floor(bandwidth / 2 / fSampleRate);
                Bandwidth = new()
                {
                    Samplerate = bandwidth,
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
            Fourier.ForwardTransform(e.Buffer, e.Length, true);

            float[] audio = new float[e.Length];
            fixed (float* src = audio) new AmDetector().Demodulate(e.Buffer, src, e.Length);

            float[] power = new float[e.Length];
            fixed (float* src = power) Fourier.SpectrumPower(e.Buffer, src, e.Length, IO.Gain);

            List<double> fftPower = new(new double[e.Length]);
            for (int i = 0; i < e.Length; i++) fftPower[i] = power[i];

            List<double> fftAudio = new(new double[e.Length]);
            for (int i = 0; i < e.Length; i++) fftAudio[i] = audio[i];

            List<double> bandwidthSlice = fftPower.GetRange(Bandwidth.Value.FromIndex, Bandwidth.Value.ToIndex - Bandwidth.Value.FromIndex);
            List<double> bandwidthFreqSlice = FrequesList.GetRange(Bandwidth.Value.FromIndex, Bandwidth.Value.ToIndex - Bandwidth.Value.FromIndex);

            //double averagePower = fftPower.Average();
            double averagePower = bandwidthSlice.Average();
            //double averageBandwidthPower = bandwidthSlice.Average();
            double averageBandwidthPower = bandwidthSlice.Max() * 0.6;
            bool isPositiveSignal = averageBandwidthPower - averagePower >= 5;

            List<double> newBandwidthSlice = bandwidthSlice.ConvertAll(item => item >= averageBandwidthPower ? item : 0);

            if (isPositiveSignal)
            {
                double freqStep = FFT.FrequencyResolution(e.Length, IO.Samplerate);
                (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(newBandwidthSlice.ToArray(), bandwidthFreqSlice.ToArray(), freqStep);
                int[] hashInts = AudioUtils.GetAudioHash(preparedPower, preparedFreqs, (int)freqStep * 100);

                string hash = "";
                for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");
                AudioBuffer.Add(hash);
            }

            string? recognizedSignal = null;
            if (AudioBuffer.Count >= 100)
            {
                //string g = JsonConvert.SerializeObject(AudioBuffer, Formatting.Indented);

                Dictionary<string, double> signalCounts = new();
                foreach (var kv in Map.Map)
                {
                    int minLength = Math.Min(AudioBuffer.Count, kv.Value.Count);
                    int maxLength = Math.Max(AudioBuffer.Count, kv.Value.Count);
                    string[] minSlice, maxSlice;

                    if (AudioBuffer.Count == minLength)
                    {
                        minSlice = AudioBuffer.ToArray();
                        maxSlice = kv.Value.ToArray();
                    }
                    else
                    {
                        minSlice = kv.Value.ToArray();
                        maxSlice = AudioBuffer.ToArray();
                    }

                    double maxAverageKorrel = -1;
                    for (int i = 0; i < maxLength - minLength + 1; i++)
                    {
                        List<double> korrels = new();
                        for (int j = 0; j < minLength; j++)
                        {
                            string hashLeft = maxSlice[i + j];
                            string hashRight = minSlice[j];

                            double? korrel = AudioUtils.CompareHashes(hashLeft, hashRight);
                            //korrels.Add(korrel == null ? 0 : Math.Abs(korrel.Value));
                            //korrels.Add(korrel == null || Math.Abs(korrel.Value) < 0.4 ? 0 : korrel.Value);
                            korrels.Add(korrel == null ? 0 : korrel.Value);
                        }

                        if (korrels.Count > 0)
                        {
                            double averageKorrel = korrels.Average();
                            if (averageKorrel > maxAverageKorrel) maxAverageKorrel = averageKorrel;
                        }
                    }

                    signalCounts[kv.Key] = maxAverageKorrel;
                }

                if (signalCounts.Keys.Count > 0)
                {
                    var countsList = signalCounts.ToList();
                    countsList.Sort((a, b) => a.Value > b.Value ? -1 : 1);
                    var maximumComparedSignal = countsList.First();
                    if (Math.Abs(maximumComparedSignal.Value) >= 0.25) recognizedSignal = maximumComparedSignal.Key;
                }

                AudioBuffer.Clear();
                BeginInvoke(() =>
                {
                    SignalNameBox.Text = recognizedSignal ?? "";
                });
            }

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 100)
            {
                double audioMax = fftAudio.Max();

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddSignalXY(FrequesList.ToArray(), fftPower.ToArray());
                    SpectrPlot.Plot.AddVerticalLine(FrequesList[FrequesList.Count / 2], Color.Red);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Left, Color.Green);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Right, Color.Green);
                    SpectrPlot.Plot.AddHorizontalLine(averagePower, Color.Chocolate);
                    SpectrPlot.Plot.AddHorizontalLine(averageBandwidthPower, Color.Black);
                    SpectrPlot.Plot.SetAxisLimitsY(-20, 100);
                    SpectrPlot.Refresh();

                    SignalPlot.Plot.Clear();
                    SignalPlot.Plot.AddSignalXY(FrequesList.ToArray(), fftAudio.ToArray());
                    SignalPlot.Plot.AddHorizontalLine(audioMax * 0.05, Color.Chocolate);
                    SignalPlot.Plot.SetAxisLimitsY(0, 1000);
                    SignalPlot.Refresh();

                    HashBox.Text = isPositiveSignal ? "Полезный" : "Шум";
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
    }
}