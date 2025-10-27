using Avalonia.Controls;
using System.Linq;
using System;
using ScannerUI.Audio;
using Avalonia.Platform.Storage;
using NAudio.Wave;
using System.Collections.Generic;
using ScottPlot;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 8192;

        private Dictionary<PlotType, AudioData> Audios { get; set; } = new()
        {
            [PlotType.Left] = new(),
            [PlotType.Right] = new(),
        };

        private enum PlotType
        {
            Left,
            Right,
        };

        private class AudioData()
        {
            public double[] Data = [];
            public double[] IntegratedSpectrum = [];
            public int Sample = 0;
            public int SampleRate = 0;
        }

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
                    ApplyAudio(PlotType.Left, Uri.UnescapeDataString(files[0].Path.AbsolutePath));
                    UpdatePlots();
                }
            };

            AlterFilePickButton.Click += async (sender, args) =>
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = "Выберите WAV-файл",
                    FileTypeFilter = [AudioUtils.WavFiles],
                    AllowMultiple = false,
                });

                if (files.Count == 1)
                {
                    AlterChoosenFileBox.Text = files[0].Name;
                    ApplyAudio(PlotType.Right, Uri.UnescapeDataString(files[0].Path.AbsolutePath));
                    UpdatePlots();
                }
            };

            LeftButton.Click += (sender, args) =>
            {
                MoveToPreviousSample(PlotType.Left);
            };

            AlterLeftButton.Click += (sender, args) =>
            {
                MoveToPreviousSample(PlotType.Right);
            };

            RightButton.Click += (sender, args) =>
            {
                MoveToNextSample(PlotType.Left);
            };

            AlterRightButton.Click += (sender, args) =>
            {
                MoveToNextSample(PlotType.Right);
            };

            ShowOriginalLines.IsCheckedChanged += (sender, args) =>
            {
                UpdateSample(PlotType.Left);
                UpdateSample(PlotType.Right);
            };

            ShowReducedLines.IsCheckedChanged += (sender, args) =>
            {
                UpdateSample(PlotType.Left);
                UpdateSample(PlotType.Right);
            };

            ShowTopPoints.IsCheckedChanged += (sender, args) =>
            {
                UpdateSample(PlotType.Left);
                UpdateSample(PlotType.Right);
            };

            ShowSimpleAveraged.IsCheckedChanged += (sender, args) =>
            {
                UpdateSample(PlotType.Left);
                UpdateSample(PlotType.Right);
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

            AlterSpectrPlot.Plot.Axes.Title.Label.FontSize = (float)FontSize;
            AlterSpectrPlot.Plot.Axes.Left.Label.FontSize = (float)FontSize;
            AlterSpectrPlot.Plot.Axes.Bottom.Label.FontSize = (float)FontSize;
            AlterSpectrPlot.Plot.Axes.Left.TickLabelStyle.FontSize = (float)FontSize;
            AlterSpectrPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = (float)FontSize;
            AlterSpectrPlot.Plot.Title("Спектр");
            AlterSpectrPlot.Plot.XLabel("Частота (Гц)");
            AlterSpectrPlot.Plot.YLabel("Мощность (дБ)");
            AlterSpectrPlot.Refresh();

            AlterPoweredPlot.Plot.Axes.Frameless();
            AlterPoweredPlot.Refresh();
        }

        private void ApplyAudio(PlotType plotType, string path)
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

            Audios[plotType].Data = floatSamples;
            Audios[plotType].SampleRate = reader.WaveFormat.SampleRate;
            MoveToSample(plotType, 0);
        }

        private void MoveToNextSample(PlotType plotType)
        {
            MoveToSample(plotType, Audios[plotType].Sample + 1);
            UpdatePlots();
        }

        private void MoveToPreviousSample(PlotType plotType)
        {
            MoveToSample(plotType, Audios[plotType].Sample - 1);
            UpdatePlots();
        }

        private void UpdateSample(PlotType plotType)
        {
            MoveToSample(plotType, Audios[plotType].Sample);
            UpdatePlots();
        }

        private void MoveToSample(PlotType plotType, int sample)
        {
            int samples = Audios[plotType].Data.Length / Resolution;
            if (samples > 0)
            {
                if (sample >= samples) sample = 0;
                else if (sample < 0) sample = samples - 1;

                var progressBox = SampleProgressBox;
                if (plotType == PlotType.Right) progressBox = AlterSampleProgressBox;

                Audios[plotType].Sample = sample;
                progressBox.Text = $"{Audios[plotType].Sample + 1} / {samples}";

                var samplesSlice = Audios[plotType].Data.Skip(Audios[plotType].Sample * Resolution).Take(Resolution).ToArray();

                ApplySample(plotType, samplesSlice);
                CalculateWhiteNoiseCorrelation(plotType);
            }
        }

        private unsafe void ApplySample(PlotType plotType, double[] powerSamples)
        {
            Plot spectrPlot = SpectrPlot.Plot;
            if (plotType == PlotType.Right) spectrPlot = AlterSpectrPlot.Plot;

            Plot powerPlot = PoweredPlot.Plot;
            if (plotType == PlotType.Right) powerPlot = AlterPoweredPlot.Plot;

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

            Audios[plotType].IntegratedSpectrum = integratedSpectrum;

            spectrPlot.Clear();
            if (ShowOriginalLines.IsChecked == true) spectrPlot.Add.Signal(power, color: Colors.Blue);
            if (ShowReducedLines.IsChecked == true) spectrPlot.Add.SignalXY([.. ramerReduced.Select(x => x.X)], [.. ramerReduced.Select(x => x.Y)], color: Colors.Orange);
            if (ShowTopPoints.IsChecked == true) spectrPlot.Add.Markers(topPoints.Select(x => x.X).ToArray(), topPoints.Select(x => x.Y).ToArray(), MarkerShape.FilledCircle, 4f, Colors.Red);
            if (ShowSimpleAveraged.IsChecked == true) spectrPlot.Add.SignalXY(topPoints.Select(x => x.X).ToArray(), topPointsSimpleAveraged, color: Colors.Green);

            spectrPlot.Axes.AutoScaleX();
            spectrPlot.Axes.AutoScaleY();

            powerPlot.Clear();
            powerPlot.Add.Signal(integratedSpectrum);
            powerPlot.Add.Line(new Coordinates(0, 0), new Coordinates(integratedSpectrum.Length, 1));

            powerPlot.Axes.AutoScaleX();
            powerPlot.Axes.AutoScaleY();
        }

        private void UpdatePlots()
        {
            SpectrPlot.Refresh();
            PoweredPlot.Refresh();

            AlterSpectrPlot.Refresh();
            AlterPoweredPlot.Refresh();

            CalculateCorrelation();
        }

        private void CalculateWhiteNoiseCorrelation(PlotType plotType)
        {
            var sample = Audios[plotType].IntegratedSpectrum;
            var text = "Корреляция сэмпла с белым шумом: ";

            if (sample.Length > 0)
            {
                var lineKoef = 1d / sample.Length;
                var lengthCurseFreqs = FftSharp.FFT.FrequencyScale(sample.Length, Audios[plotType].SampleRate);
                var mae = sample.Select((s, index) => Math.Abs(s - lineKoef * index)).Sum() / sample.Length;
                // Алгоритм MAE - среднее абсолютное отклонение

                text += mae.ToString();
            }
            else text += "0";

            if (plotType == PlotType.Left) SampleCorrelationBox.Text = text;
            else if (plotType == PlotType.Right) AlterSampleCorrelationBox.Text = text;
        }

        private void CalculateCorrelation()
        {
            var samplesSlice = Audios[PlotType.Left].IntegratedSpectrum;
            var alterSamplesSlice = Audios[PlotType.Right].IntegratedSpectrum;

            if (samplesSlice.Length > 0 && alterSamplesSlice.Length > 0)
            {
                var correlation = AudioUtils.Correlation(samplesSlice, alterSamplesSlice);
                CorrelationText.Text = correlation.ToString();
            }
            else CorrelationText.Text = "0";
        }
    }
}