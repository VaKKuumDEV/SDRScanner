using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using SDRSharp.Radio.PortAudio;
using FftSharp;
using Newtonsoft.Json;

namespace Scanner
{
    public partial class MainForm : Form
    {
        public const int RESOLUTION = 8192;
        public const int BUFFER_LENGTH = 50;
        public const double KORREL_DOPUSK = 0.5;

        private static RtlDevice? IO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = new();
        private List<AudioDevice> AudioDevices { get; } = new();
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private List<double> FrequesList { get; set; } = new();
        private List<string> AudioBuffer { get; set; } = new();
        private BandwidthInfo? Bandwidth { get; set; } = null;
        private int Iter { get; set; } = 0;
        private SignalsMap Map { get; }
        private string? RecordingSignal { get; set; } = null;
        private List<string> RecordingBuffer { get; set; } = new();

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
                StartSignalRecButton.Show();

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

            List<double> bandwidthSlice = fftPower.GetRange(Bandwidth.Value.FromIndex, Bandwidth.Value.ToIndex - Bandwidth.Value.FromIndex);
            List<double> bandwidthFreqSlice = FrequesList.GetRange(Bandwidth.Value.FromIndex, Bandwidth.Value.ToIndex - Bandwidth.Value.FromIndex);

            double averagePower = fftPower.Average() * 1.5;
            int avPreCount = bandwidthSlice.Where(item => item >= averagePower).Count();
            double avPrePercent = ((double)avPreCount) / bandwidthSlice.Count;
            bool isPositiveSignal = avPrePercent >= 0.04;

            List<double> newBandwidthSlice = bandwidthSlice.ConvertAll(item => item >= averagePower ? item : 0);

            if (isPositiveSignal)
            {
                double freqStep = FFT.FrequencyResolution(e.Length, IO.Samplerate);
                (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(newBandwidthSlice.ToArray(), bandwidthFreqSlice.ToArray(), freqStep);
                int[] hashInts = AudioUtils.GetAudioHash(preparedPower);

                string hash = "";
                for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");

                if (RecordingSignal != null) RecordingBuffer.Add(hash);
                else AudioBuffer.Add(hash);
            }

            string? recognizedSignal = null;
            if (AudioBuffer.Count >= BUFFER_LENGTH && RecordingSignal == null)
            {
                Dictionary<string, KeyValuePair<int, double>> signalCounts = new();
                foreach (var kv in Map.Map)
                {
                    if (kv.Value.Count == 0) continue;
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

                    int maxKorrelsCount = 0;
                    double avKorrel = 0;
                    for (int i = 0; i < maxLength - minLength + 1; i++)
                    {
                        List<double> korrels = new();
                        for (int j = 0; j < minLength; j++)
                        {
                            string hashLeft = maxSlice[i + j];
                            string hashRight = minSlice[j];

                            double? korrel = AudioUtils.CompareHashes(hashLeft, hashRight);
                            if (korrel != null && korrel.Value >= KORREL_DOPUSK) korrels.Add(korrel.Value);
                        }

                        if (korrels.Count > maxKorrelsCount)
                        {
                            maxKorrelsCount = korrels.Count;
                            avKorrel = korrels.Average();
                        }
                    }

                    signalCounts[kv.Key] = new(maxKorrelsCount, avKorrel);
                }

                if (signalCounts.Keys.Count > 0)
                {
                    var countsList = signalCounts.ToList();
                    countsList.Sort((a, b) => a.Value.Key == b.Value.Key ? (a.Value.Value > b.Value.Value ? -1 : 1) : (a.Value.Key > b.Value.Key ? -1 : 1));
                    var maximumComparedSignal = countsList.First();

                    double compPercent = ((double)maximumComparedSignal.Value.Key) / BUFFER_LENGTH;
                    if (compPercent > 0.2) recognizedSignal = maximumComparedSignal.Key;
                }

                AudioBuffer.Clear();
                BeginInvoke(() =>
                {
                    SignalNameBox.Text = recognizedSignal ?? "Не опознан";
                });
            }

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 200)
            {
                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddSignalXY(FrequesList.ToArray(), fftPower.ToArray());
                    SpectrPlot.Plot.AddVerticalLine(FrequesList[FrequesList.Count / 2], Color.Red);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Left, Color.Green);
                    SpectrPlot.Plot.AddVerticalLine(Bandwidth.Value.Right, Color.Green);
                    SpectrPlot.Plot.AddHorizontalLine(averagePower, Color.Chocolate);
                    SpectrPlot.Plot.SetAxisLimitsY(-20, 100);
                    SpectrPlot.Refresh();

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
                    if (!Map.Map.ContainsKey(RecordingSignal)) Map.Map[RecordingSignal] = new();
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