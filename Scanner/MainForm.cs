using SDRSharp.RTLSDR;
using SDRSharp.Radio;
using Scanner.Audio;
using FftSharp;
using System.Drawing;

namespace Scanner
{
    public partial class MainForm : Form
    {
        public const int RESOLUTION = 8192;
        public const int BUFFER_LENGTH = 50;
        public const double KORREL_DOPUSK = 0.5;

        private IFrontendController? Source { get; set; } = null;
        private WorkingStatuses Status { get; set; } = WorkingStatuses.NOT_INIT;
        private DateTime SignalTime { get; set; } = DateTime.Now;
        private double[] FrequesList { get; set; } = [];
        private int NoiseOffset { get => Convert.ToInt32(NoiseOffsetBox.Value); set => NoiseOffsetBox.Value = value; }

        public enum WorkingStatuses
        {
            NOT_INIT,
            STARTED,
            STOPPED,
        };

        unsafe public MainForm()
        {
            InitializeComponent();

            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            AddSourceBox.Click += new((sender, args) =>
            {
                ChooseTypeForm chooseForm = new();
                chooseForm.ShowDialog(this);

                if (chooseForm.ChoosenType == ChooseTypeForm.SdrTypes.USB)
                {
                    RtlSdrForm form = new();
                    form.ShowDialog(this);

                    if (form.RtlSdrIO is { } source)
                    {
                        Source = source;
                        SourceBox.Text = "(USB) " + source.Device?.Name;
                        Status = WorkingStatuses.STOPPED;
                    }
                }
                else if (chooseForm.ChoosenType == ChooseTypeForm.SdrTypes.TCP)
                {
                    RtlTcpForm form = new();
                    form.ShowDialog(this);

                    if (form.RtlTcpIO is { } source)
                    {
                        Source = source;
                        SourceBox.Text = "(TCP)";
                        Status = WorkingStatuses.STOPPED;
                    }
                }
            });

            FreqBox.ValueChanged += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED)
                {
                    Stop();
                    Start();
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                if (Status == WorkingStatuses.STARTED) Stop();
                else if (Status == WorkingStatuses.STOPPED) Start();
            });
        }

        private void Stop()
        {
            Source?.Stop();

            Status = WorkingStatuses.STOPPED;
            ControlButton.Text = "Запуск";
            AddSourceBox.Enabled = true;
        }

        private unsafe void Start()
        {
            if (Source != null)
            {
                uint freq = (uint)FreqBox.Value * 1000;

                FrequesList = new double[RESOLUTION];
                double fSampleRate = FFT.FrequencyResolution(RESOLUTION, Source.Samplerate);
                for (int i = 0; i < FrequesList.Length; i++) FrequesList[i] = freq - (Source.Samplerate / 2) + (i * fSampleRate);

                SignalTime = DateTime.Now;

                Source.Frequency = freq;
                Source.Start(IO_SamplesAvailable);

                ControlButton.Text = "Остановить";
                AddSourceBox.Enabled = false;
                SpectrPlot.Plot.Clear();

                Status = WorkingStatuses.STARTED;
            }
            else MessageBox.Show("Не загружен источник", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private unsafe void IO_SamplesAvailable(IFrontendController sender, SDRSharp.Radio.Complex* data, int len)
        {
            if (Source == null) return;
            //Fourier.ForwardTransform(data, len);

            int gain = 0;
            if (Source is RtlSdrIO rtlsdr && rtlsdr.Device != null) gain = rtlsdr.Device.Gain;

            float[] power = new float[len];
            float[] simpleAveraged = new float[len];
            float[] integratedPower = new float[len];
            fixed (float* srcPower = power)
            {
                Fourier.SpectrumPower(data, srcPower, len, gain);
                fixed (float* averagedSrc = simpleAveraged)
                {
                    AudioUtils.SimpleAverage(srcPower, averagedSrc, len, 30);
                }

                fixed (float* cumSrc = integratedPower)
                {
                    AudioUtils.CumulativeSum(srcPower, cumSrc, len);
                    AudioUtils.IntegratedSpectrum(cumSrc, len);
                }
            }

            AudioUtils.Point[] points = new AudioUtils.Point[len], filteredPoints;
            for (int i = 0; i < simpleAveraged.Length; i++) points[i] = new(FrequesList[i], simpleAveraged[i]);
            fixed (AudioUtils.Point* pointsSrc = points)
            {
                filteredPoints = RamerDouglasPeucker.Reduce(pointsSrc, 3, len);
            }

            filteredPoints[0].Pos = AudioUtils.Point.Position.Low;
            for (int i = 1; i < filteredPoints.Length; i++)
            {
                if (filteredPoints[i].Y >= filteredPoints[i - 1].Y)
                {
                    filteredPoints[i].Pos = AudioUtils.Point.Position.Top;
                    if (filteredPoints[i - 1].Pos == AudioUtils.Point.Position.Top) filteredPoints[i - 1].Pos = AudioUtils.Point.Position.None;
                }
                else if (filteredPoints[i].Y < filteredPoints[i - 1].Y)
                {
                    filteredPoints[i].Pos = AudioUtils.Point.Position.Low;
                    if (filteredPoints[i - 1].Pos == AudioUtils.Point.Position.Low) filteredPoints[i - 1].Pos = AudioUtils.Point.Position.None;
                }
            }

            var topPoints = filteredPoints.Where(p => p.Pos == AudioUtils.Point.Position.Top);
            var noiseLevel = topPoints.Select(p => p.Y).Average() + NoiseOffset;
            var positiveCount = topPoints.Where(p => p.Y - noiseLevel > NoiseOffset * 2).Count();

            if ((DateTime.Now - SignalTime).TotalMilliseconds >= 100)
            {
                BeginInvoke(() =>
                {
                    SpectrPlot.Plot.Clear();
                    SpectrPlot.Plot.Add.SignalXY(FrequesList.ToArray(), simpleAveraged);
                    SpectrPlot.Plot.Add.SignalXY(filteredPoints.Select(p => p.X).ToArray(), filteredPoints.Select(p => p.Y).ToArray());
                    SpectrPlot.Plot.Add.VerticalLine(FrequesList[RESOLUTION / 2], color: ScottPlot.Color.FromColor(Color.Red));

                    SpectrPlot.Plot.Add.HorizontalLine(noiseLevel, color: ScottPlot.Color.FromColor(Color.Aqua));

                    SpectrPlot.Plot.Axes.AutoScaleX();
                    SpectrPlot.Plot.Axes.SetLimitsY(noiseLevel - 30, noiseLevel + 30);
                    SpectrPlot.Refresh();

                    NoisePlot.Plot.Clear();
                    NoisePlot.Plot.Add.SignalXY(FrequesList, integratedPower);
                    NoisePlot.Plot.Add.Line(new(FrequesList.First(), 0), new(FrequesList.Last(), 1));
                    NoisePlot.Plot.Axes.AutoScaleX();
                    NoisePlot.Plot.Axes.AutoScaleY();
                    NoisePlot.Refresh();

                    SignalTypeBox.Text = "(" + positiveCount + ") " + (positiveCount == 0 ? "Шум" : "Полезный сигнал");
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