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
            SpectrPlot.Plot.Add.Signal(power);

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