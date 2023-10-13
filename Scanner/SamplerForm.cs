using Scanner.Audio;
using System.ComponentModel;

namespace Scanner
{
    public partial class SamplerForm : Form
    {
        private SignalsMap Map { get; set; }

        public SamplerForm()
        {
            string filePath = new DirectoryInfo(Environment.CurrentDirectory + "/Samples.json").FullName;
            Map = new(filePath);

            InitializeComponent();
            ViewBase();
        }

        private void ViewBase()
        {
            SamplesBox.Items.Clear();
            foreach (var kv in Map.Map) SamplesBox.Items.Add(kv.Key + ": " + kv.Value.Count);
        }

        private void SerializeButton_Click(object sender, EventArgs e)
        {
            ChooseFilesDialog.ShowDialog();
        }

        private async void ChooseFilesDialog_FileOk(object sender, CancelEventArgs e)
        {
            ContentPanel.Enabled = false;
            Cursor = Cursors.WaitCursor;

            await Task.Run(() =>
            {
                foreach (string fileName in ChooseFilesDialog.FileNames)
                {
                    FileInfo file = new(fileName);
                    (double[] audio, int sampleRate, int bytesPerSample, int totalTime) = AudioUtils.ReadAudioFile(new(fileName));
                    KeyValuePair<double[], double[]>[] samplesData = AudioUtils.MakeFFT(audio, sampleRate, totalTime);
                    Task[] hashTasks = new Task[samplesData.Length];

                    List<string> samplesList = new(new string[samplesData.Length]);
                    for (int sample = 0; sample < samplesData.Length; sample++)
                    {
                        hashTasks[sample] = new TaskFactory().StartNew((sampleObjectData) =>
                        {
                            var sampleData = ((NeuroForm.SampleData?)sampleObjectData) ?? throw new ArgumentNullException(nameof(sampleObjectData));

                            (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(sampleData.data.Key, sampleData.data.Value);
                            int[] hashInts = AudioUtils.GetAudioHash(preparedPower, preparedFreqs);

                            string hash = "";
                            for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");
                            samplesList[sampleData.index] = hash;
                        }, new NeuroForm.SampleData { data = samplesData[sample], index = sample });
                    }

                    Task.WaitAll(hashTasks);
                    Map.AddSamples(Path.GetFileNameWithoutExtension(file.Name), samplesList);
                }

                Map.Save();
            });

            ViewBase();
            ContentPanel.Enabled = true;
            Cursor = Cursors.Arrow;
        }
    }
}
