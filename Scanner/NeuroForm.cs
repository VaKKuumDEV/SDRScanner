﻿using Scanner.Audio;
using System.Collections;
using System.ComponentModel;
using System.Security.Policy;

namespace Scanner
{
    public partial class NeuroForm : Form
    {
        struct SampleInfo
        {
            public double[] power;
            public double[] freqs;
            public double[] preparedPower;
            public double[] preparedFreqs;
            public string hash;
        };

        private List<SampleInfo>? SamplesData { get; set; } = null;
        private int? CurrentSelection { get; set; } = null;

        public NeuroForm()
        {
            InitializeComponent();

            SignalPlot.Plot.Title("БПФ без фильтрации");
            SignalPlot.Plot.XLabel("Частота (Гц)");
            SignalPlot.Plot.YLabel("Амплитуда (дБ)");

            PreparedSignalPlot.Plot.Title("БПФ с фильтром");
            PreparedSignalPlot.Plot.XLabel("Частота (Гц)");
            PreparedSignalPlot.Plot.YLabel("Амплитуда (дБ)");
        }

        private void OpenSignalButton_Click(object sender, EventArgs e)
        {
            OpenSignalDialog.ShowDialog(this);
        }

        private async void OpenSignalDialog_FileOk(object sender, CancelEventArgs e)
        {
            ContentPanel.Enabled = false;
            Cursor = Cursors.WaitCursor;

            CurrentSelection = null;
            BeginInvoke(() =>
            {
                HashListbox.Items.Clear();
            });

            await Task.Run(() =>
            {
                try
                {
                    (double[] audio, int sampleRate, int bytesPerSample, int totalTime) = AudioUtils.ReadAudioFile(new(OpenSignalDialog.FileName));
                    var samplesData = AudioUtils.MakeFFT(audio, sampleRate, totalTime);

                    SamplesData = new();
                    for (int sample = 0; sample < samplesData.Length; sample++)
                    {
                        (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(samplesData[sample].Key, samplesData[sample].Value);
                        int[] hashInts = AudioUtils.GetAudioHash(preparedPower, preparedFreqs);

                        string hash = "";
                        for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");
                        SamplesData.Add(new() { power = samplesData[sample].Key, freqs = samplesData[sample].Value, preparedPower = preparedPower, preparedFreqs = preparedFreqs, hash = hash });

                        BeginInvoke(() =>
                        {
                            HashListbox.Items.Add("Sample " + sample + ": " + hash);
                        });
                    }

                    if (SamplesData.Count > 0)
                    {
                        CurrentSelection = 0;
                        InitSelection();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            ContentPanel.Enabled = true;
            Cursor = Cursors.Arrow;
        }

        public void InitSelection()
        {
            if (CurrentSelection == null || !CurrentSelection.HasValue) return;
            if (CurrentSelection.Value < 0) return;
            if (SamplesData == null || !SamplesData.Any()) return;
            if (CurrentSelection.Value >= SamplesData.Count) return;

            var kv = SamplesData[CurrentSelection.Value];

            BeginInvoke(() =>
            {
                SignalPlot.Plot.Clear();
                PreparedSignalPlot.Plot.Clear();

                SignalPlot.Plot.AddSignalXY(kv.freqs, kv.power);
                PreparedSignalPlot.Plot.AddSignalXY(kv.preparedFreqs, kv.preparedPower, Color.Green);

                SignalPlot.Refresh();
                PreparedSignalPlot.Refresh();

                ListingLabel.Text = (CurrentSelection.Value + 1) + "/" + SamplesData.Count;
                HashTextbox.Text = kv.hash;
            });
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (CurrentSelection != null && CurrentSelection.Value > 0)
            {
                CurrentSelection--;
                InitSelection();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (CurrentSelection != null && SamplesData != null && CurrentSelection.Value < SamplesData.Count - 1)
            {
                CurrentSelection++;
                InitSelection();
            }
        }
    }
}
