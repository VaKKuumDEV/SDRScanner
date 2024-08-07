using Scanner.Audio;
using System.Collections;
using System.ComponentModel;
using System.Security.Policy;

namespace Scanner
{
    public partial class NeuroForm : Form
    {
        public struct SampleInfo
        {
            public double[] power;
            public double[] freqs;
            public double[] preparedPower;
            public double[] preparedFreqs;
            public string hash;
        };

        public struct SampleData
        {
            public KeyValuePair<double[], double[]> data;
            public int index;
        }

        private List<SampleInfo>? SamplesData { get; set; } = null;
        private int? CurrentSelection { get; set; } = null;
        private SignalsMap Map { get; }

        public NeuroForm()
        {
            Map = new(new DirectoryInfo(Environment.CurrentDirectory + "/Samples.json").FullName);
            InitializeComponent();

            SignalPlot.Plot.Title("БПФ без преобразования");
            SignalPlot.Plot.XLabel("Частота (Гц)");
            SignalPlot.Plot.YLabel("Амплитуда (дБ)");

            PreparedSignalPlot.Plot.Title("БПФ с преобразованием");
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
            await Task.Run(() =>
            {
                try
                {
                    (double[] audio, int sampleRate, int bytesPerSample, int totalTime) = AudioUtils.ReadAudioFile(new(OpenSignalDialog.FileName));
                    var samplesData = AudioUtils.MakeFFT(audio, sampleRate, totalTime);

                    SamplesData = new(new SampleInfo[samplesData.Length]);
                    BeginInvoke(() =>
                    {
                        FreqBox.Text = "SR: " + sampleRate + "Hz";
                        PrintCorrectSamples();
                    });

                    Task[] hashTasks = new Task[samplesData.Length];
                    for (int sample = 0; sample < samplesData.Length; sample++)
                    {
                        hashTasks[sample] = new TaskFactory().StartNew((sampleObjectData) =>
                        {
                            var sampleData = ((SampleData?)sampleObjectData) ?? throw new ArgumentNullException(nameof(sampleObjectData));

                            (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(sampleData.data.Key, sampleData.data.Value);
                            int[] hashInts = AudioUtils.GetAudioHash(preparedPower);

                            string hash = "";
                            for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");
                            SamplesData[sampleData.index] = new() { power = sampleData.data.Key, freqs = sampleData.data.Value, preparedPower = preparedPower, preparedFreqs = preparedFreqs, hash = hash };

                            BeginInvoke(() =>
                            {
                                PrintCorrectSamples();
                            });
                        }, new SampleData { data = samplesData[sample], index = sample });
                    }

                    Task.WaitAll(hashTasks);
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
            if (SamplesData == null || SamplesData.Count == 0) return;
            if (CurrentSelection.Value >= SamplesData.Count) return;

            var kv = SamplesData[CurrentSelection.Value];

            BeginInvoke(() =>
            {
                SignalPlot.Plot.Clear();
                PreparedSignalPlot.Plot.Clear();

                SignalPlot.Plot.Add.SignalXY(kv.freqs, kv.power);
                PreparedSignalPlot.Plot.Add.SignalXY(kv.preparedFreqs, kv.preparedPower, color: ScottPlot.Color.FromColor(Color.Green));

                SignalPlot.Refresh();
                PreparedSignalPlot.Refresh();

                ListingLabel.Text = (CurrentSelection.Value + 1) + "/" + SamplesData.Count;
                HashTextbox.Text = kv.hash;
            });
        }

        private void PrintCorrectSamples()
        {
            HashListbox.Items.Clear();
            if (SamplesData == null) return;
            for (int i = 0; i < SamplesData.Count; i++)
            {
                HashListbox.Items.Add("Sample " + i.ToString("00") + ": " + SamplesData[i].hash);
            }
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

        private void SignalsBaseButton_Click(object sender, EventArgs e)
        {
            SamplerForm form = new();
            form.ShowDialog(this);

            Map.Reload();
        }

        private void RecognizeButton_Click(object sender, EventArgs e)
        {
            if (CurrentSelection == null) return;
            if (SamplesData == null) return;
            SampleInfo info = SamplesData[CurrentSelection.Value];

            List<KeyValuePair<string, double>> percents = [];
            foreach (var kv in Map.Map)
            {
                foreach (var valHash in kv.Value)
                {
                    double? percent = AudioUtils.CompareHashes(info.hash, valHash);
                    if (percent != null) percents.Add(new(kv.Key, Math.Abs(percent.Value)));
                }
            }

            if (percents.Count > 0)
            {
                percents.Sort((a, b) => a.Value > b.Value ? -1 : 1);
                KeyValuePair<string, double> maxPercentPair = percents.First();

                MessageBox.Show("Сигнал " + maxPercentPair.Key + ": " + maxPercentPair.Value);
            }
            else
            {
                MessageBox.Show("Для данного сигнала  нет совпадений", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecognizeAllButton_Click(object sender, EventArgs e)
        {
            if (CurrentSelection == null) return;
            if (SamplesData == null) return;

            Dictionary<string, int> counts = [];
            foreach (SampleInfo info in SamplesData)
            {
                List<KeyValuePair<string, double>> percents = [];
                foreach (var kv in Map.Map)
                {
                    foreach (var valHash in kv.Value)
                    {
                        double? percent = AudioUtils.CompareHashes(info.hash, valHash);
                        if (percent != null) percents.Add(new(kv.Key, Math.Abs(percent.Value)));
                    }
                }

                if (percents.Count > 0)
                {
                    percents.Sort((a, b) => a.Value > b.Value ? -1 : 1);
                    KeyValuePair<string, double> maxPercent = percents.First();

                    if (!counts.TryGetValue(maxPercent.Key, out int value)) counts[maxPercent.Key] = 1;
                    else counts[maxPercent.Key] = ++value;
                }
            }

            if (counts.Keys.Count > 0)
            {
                var countsList = counts.ToList();
                countsList.Sort((a, b) => a.Value > b.Value ? -1 : 1);
                var maximum = countsList.First();
                MessageBox.Show("Сигнал " + maximum.Key + ": " + maximum.Value);
            }
            else
            {
                MessageBox.Show("Для данного сигнала  нет совпадений", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
