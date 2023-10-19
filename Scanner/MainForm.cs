using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using System.Numerics;
using System.Media;
using SDRSharp.Radio.PortAudio;

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
        private WavePlayer? Player { get; set; }

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        unsafe public MainForm()
        {
            InitializeComponent();
            AdditionalPanel.Hide();

            SpectrPlot.Plot.Title("Осциллограмма");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            SignalPlot.Plot.Title("Сигнал");
            SignalPlot.Plot.XLabel("Время (мс)");
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

            AudioDevicesBox.SelectedIndexChanged += new((sender, args) =>
            {
                int index = AudioDevicesBox.SelectedIndex;
                if (index != -1 && index < AudioDevices.Count)
                {
                    if (Player == null) return;

                    AudioDevice device = AudioDevices[index];
                    Player = new(device.Index, 44100, 100, (buffer, length) =>
                    {
                        
                    });
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });

            LoadDevicesList();
            LoadAudioDevicesList();
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

        private void LoadAudioDevicesList()
        {
            List<AudioDevice> devices = AudioDevice.GetDevices(DeviceDirection.Output);
            AudioDevices.Clear();
            AudioDevices.AddRange(devices);

            AudioDevicesBox.Items.Clear();
            foreach (AudioDevice device in AudioDevices) AudioDevicesBox.Items.Add("(#" + device.Index + ") " + device.Name);

            if (AudioDevices.Count > 0) AudioDevicesBox.SelectedIndex = 0;
        }

        private void Stop()
        {
            Status = WorkingStatuses.STOPPED;
            IO?.Stop();

            AdditionalPanel.Hide();
            ControlButton.Text = "Запуск";
        }

        private void Start()
        {
            if (IO != null)
            {
                SignalTime = DateTime.Now;
                uint freq = (uint)FreqBox.Value * 1000;
                int gain = 0;

                if (GainBox.SelectedIndex != -1) gain = IO.SupportedGains[GainBox.SelectedIndex];

                IO.Frequency = freq;
                IO.Gain = gain;
                IO.Start();
                IO.UseTunerAGC = true;

                FrequesList = new(new double[8192]);
                double fSampleRate = IO.Samplerate / FrequesList.Count;
                for (int i = 0; i < FrequesList.Count; i++) FrequesList[i] = IO.Frequency - (IO.Samplerate / 2) + (i * fSampleRate);

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
            float* power = stackalloc float[e.Length];
            Fourier.InverseTransform(e.Buffer, e.Length);
            Fourier.SpectrumPower(e.Buffer, power, e.Length);

            float* audio = stackalloc float[e.Length];
            FmDetector det = new()
            {
                Mode = FmMode.Wide,
                SampleRate = IO.Samplerate
            };
            det.Demodulate(e.Buffer, audio, e.Length);

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 100)
            {
                List<double> fftPower = new(new double[e.Length]);
                for (int i = 0; i < e.Length; i++) fftPower[i] = power[i];

                List<double> fftAudio = new(new double[e.Length]);
                for (int i = 0; i < e.Length; i++) fftAudio[i] = audio[i];
                double[] filtered = FftSharp.Filter.LowPass(fftAudio.ToArray(), IO.Samplerate, maxFrequency: 2000);

                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.AddSignalXY(FrequesList.ToArray(), fftPower.ToArray());
                    SpectrPlot.Plot.AddVerticalLine(FrequesList[FrequesList.Count / 2], Color.Red);
                    SpectrPlot.Plot.SetAxisLimitsY(-130, 20);
                    SpectrPlot.Refresh();

                    SignalPlot.Plot.Clear();
                    SignalPlot.Plot.AddSignal(filtered);
                    SignalPlot.Plot.SetAxisLimitsY(-1E-5, 1E-5);
                    SignalPlot.Refresh();

                    AveragePowerBox.Text = "NaN";
                });
                SignalTime = DateTime.Now;
            }
        }
    }
}