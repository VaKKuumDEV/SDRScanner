using Avalonia.Controls;
using System.Linq;
using System;
using ScannerUI.Audio;
using Avalonia.Platform.Storage;
using NAudio.Wave;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 8192;

        private double[] AudioSamples = [];
        private int SampleNum { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();

            FilePickButton.Click += async (sender, args) =>
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = "Выберите WAV-файл",
                    FileTypeFilter = [AudioUtils.WavFiles],
                    AllowMultiple = false,
                });

                if (files.Count == 1)
                {
                    ChoosenFileBox.Text = files[0].Name;
                    ApplyAudio(Uri.UnescapeDataString(files[0].Path.AbsolutePath));
                }
            };

            LeftButton.Click += (sender, args) =>
            {
                MoveToSample(SampleNum - 1);
            };

            RightButton.Click += (sender, args) =>
            {
                MoveToSample(SampleNum + 1);
            };

            ShowOriginalLines.IsCheckedChanged += (sender, args) =>
            {
                MoveToSample(SampleNum);
            };

            ShowReducedLines.IsCheckedChanged += (sender, args) =>
            {
                MoveToSample(SampleNum);
            };

            ShowTopPoints.IsCheckedChanged += (sender, args) =>
            {
                MoveToSample(SampleNum);
            };

            ShowSimpleAveraged.IsCheckedChanged += (sender, args) =>
            {
                MoveToSample(SampleNum);
            };

            SpectrPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            SpectrPlot.Plot.Title("Спектр");
            SpectrPlot.Plot.XLabel("Частота (Гц)");
            SpectrPlot.Plot.YLabel("Мощность (дБ)");
            SpectrPlot.Refresh();

            PoweredPlot.Plot.Axes.Frameless();
            PoweredPlot.Refresh();
        }

        private void ApplyAudio(string path)
        {
            using WaveFileReader reader = new(path);
            byte[] bytesBuffer = new byte[reader.Length];
            int read = reader.Read(bytesBuffer, 0, bytesBuffer.Length);

            var floatSamples = new double[read / 2];
            for (int sampleIndex = 0; sampleIndex < read / 2; sampleIndex++)
            {
                var intSampleValue = BitConverter.ToInt16(bytesBuffer, sampleIndex * 2);
                floatSamples[sampleIndex] = intSampleValue / 32768.0;
            }

            var window = new FftSharp.Windows.Hanning();
            window.ApplyInPlace(floatSamples);

            AudioSamples = floatSamples;
            MoveToSample(0);
        }

        private void MoveToSample(int sample)
        {
            int samples = AudioSamples.Length / Resolution;
            if (samples > 0)
            {
                if (sample >= samples) sample = 0;
                else if (sample < 0) sample = samples - 1;

                SampleNum = sample;
                SampleProgressBox.Text = $"{SampleNum + 1} / {samples}";

                var samplesSlice = AudioSamples.Skip(SampleNum * Resolution).Take(Resolution).ToArray();
                ApplySample(samplesSlice);
            }
        }

        private unsafe void ApplySample(double[] powerSamples)
        {
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(powerSamples);
            double[] power = FftSharp.FFT.Power(spectrum);

            var powerPoints = new AudioUtils.Point[power.Length];
            for (int i = 0; i < power.Length; i++) powerPoints[i] = new(i, power[i]);
            AudioUtils.Point[] ramerReduced;
            fixed (AudioUtils.Point* powerPointsSrc = powerPoints)
            {
                ramerReduced = RamerDouglasPeucker.Reduce(powerPointsSrc, 9f, powerPoints.Length);
            }

            ramerReduced[0].Pos = AudioUtils.Point.Position.Low;
            for (int i = 1; i < ramerReduced.Length; i++)
            {
                if (ramerReduced[i].Y >= ramerReduced[i - 1].Y)
                {
                    ramerReduced[i].Pos = AudioUtils.Point.Position.Top;
                    if (ramerReduced[i - 1].Pos == AudioUtils.Point.Position.Top) ramerReduced[i - 1].Pos = AudioUtils.Point.Position.None;
                }
                else if (ramerReduced[i].Y < ramerReduced[i - 1].Y)
                {
                    ramerReduced[i].Pos = AudioUtils.Point.Position.Low;
                    if (ramerReduced[i - 1].Pos == AudioUtils.Point.Position.Low) ramerReduced[i - 1].Pos = AudioUtils.Point.Position.None;
                }
            }
            var topPoints = ramerReduced.Where(p => p.Pos == AudioUtils.Point.Position.Top).ToArray();

            var topPointsSimpleAveraged = new double[topPoints.Length];
            fixed (double* topPointsSimpleAveragedSrc = topPointsSimpleAveraged)
            {
                fixed (double* topPointsSrc = topPoints.Select(p => p.Y).ToArray())
                {
                    AudioUtils.SimpleAverage(topPointsSrc, topPointsSimpleAveragedSrc, topPoints.Length, 5);
                }
            }

            double[] integratedSpectrum = new double[power.Length];
            fixed (double* powerSrc = power)
            {
                fixed (double* integratedSpectrumSrc = integratedSpectrum)
                {
                    AudioUtils.CumulativeSum(powerSrc, integratedSpectrumSrc, power.Length);
                    AudioUtils.IntegratedSpectrum(integratedSpectrumSrc, power.Length);
                }
            }

            SpectrPlot.Plot.Clear();
            if (ShowOriginalLines.IsChecked == true) SpectrPlot.Plot.Add.Signal(power, color: ScottPlot.Colors.LightBlue);
            if (ShowReducedLines.IsChecked == true) SpectrPlot.Plot.Add.SignalXY([.. ramerReduced.Select(x => x.X)], [.. ramerReduced.Select(x => x.Y)], color: ScottPlot.Colors.Orange);
            if (ShowTopPoints.IsChecked == true) SpectrPlot.Plot.Add.Markers(topPoints.Select(x => x.X).ToArray(), topPoints.Select(x => x.Y).ToArray(), ScottPlot.MarkerShape.FilledCircle, 4f, ScottPlot.Colors.Red);
            if (ShowSimpleAveraged.IsChecked == true) SpectrPlot.Plot.Add.SignalXY(topPoints.Select(x => x.X).ToArray(), topPointsSimpleAveraged, color: ScottPlot.Colors.Green);

            SpectrPlot.Plot.Axes.AutoScaleX();
            SpectrPlot.Plot.Axes.AutoScaleY();
            SpectrPlot.Refresh();

            PoweredPlot.Plot.Clear();
            PoweredPlot.Plot.Add.Signal(integratedSpectrum);

            PoweredPlot.Plot.Axes.AutoScaleX();
            PoweredPlot.Plot.Axes.AutoScaleY();
            PoweredPlot.Refresh();
        }
    }
}