using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using SDRSharp.Radio.PortAudio;
using FftSharp;
using SDRSharp.RTLTCP;
using Complex = SDRSharp.Radio.Complex;

namespace Scanner
{
    public partial class TcpForm : Form
    {
        private RtlTcpIO IO { get; set; } = new();
        private WorkingStatuses Status { get; set; } = WorkingStatuses.STOPPED;
        private DateTime SignalTime { get; set; } = DateTime.Now;

        public enum WorkingStatuses
        {
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

        unsafe public TcpForm()
        {
            InitializeComponent();
            AdditionalPanel.Hide();

            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            IO.ShowSettingGUI(this);

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });
        }

        private void Stop()
        {
            Status = WorkingStatuses.STOPPED;
            IO?.Stop();

            AdditionalPanel.Hide();
            ControlButton.Text = "Запуск";
        }

        private unsafe void Start()
        {
            uint freq = (uint)FreqBox.Value * 1000;

            IO.Frequency = freq;
            IO.Start(Receive);

            ControlButton.Text = "Остановить";
            SpectrPlot.Plot.Clear();
            AdditionalPanel.Show();

            Status = WorkingStatuses.STARTED;
            SignalTime = DateTime.Now;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Status == WorkingStatuses.STARTED) Stop();
        }

        private unsafe void Receive(IFrontendController sender, Complex* data, int len)
        {
            float[] power = new float[len];
            fixed (float* srcPower = power)
            {
                Fourier.SpectrumPower(data, srcPower, len);
            }

            float average = power.Average();

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 500)
            {
                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.Signal(power);
                    SpectrPlot.Plot.Add.HorizontalLine(average, color: ScottPlot.Color.FromColor(Color.Chocolate));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.AutoScaleY();
                    SpectrPlot.Refresh();
                });
                SignalTime = DateTime.Now;
            }
        }
    }
}