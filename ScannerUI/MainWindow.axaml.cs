using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Collections.Concurrent;
using ScannerUI.Audio;

namespace ScannerUI
{
    public partial class MainWindow : Window
    {
        public const int Resolution = 8192;
        private const int AverageCount = 50; // Количество усреднений

        private readonly Lock _lock = new();
        private ConcurrentQueue<float[]> SpectrumBuffer = [];
        private float AverageNoiseLevel = 0;

        public MainWindow()
        {
            InitializeComponent();

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

        private float[] GetSummarizedBuffer()
        {
            var buffer = SpectrumBuffer.ToArray();
            var resultBuffer = new float[Resolution];

            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < buffer.Length; j++)
                {
                    resultBuffer[i] += buffer[j][i];
                }
            }

            for (int i = 0; i < Resolution; i++)
            {
                resultBuffer[i] /= buffer.Length;
            }

            return resultBuffer;
        }
    }
}