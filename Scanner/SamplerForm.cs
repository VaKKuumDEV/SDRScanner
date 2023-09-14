using FftSharp;
using Scanner.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Scanner.NeuroForm;

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
                    var samplesData = AudioUtils.MakeFFT(audio, sampleRate, totalTime);

                    List<string> samplesList = new();
                    for (int sample = 0; sample < samplesData.Length; sample++)
                    {
                        (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(samplesData[sample].Key, samplesData[sample].Value);
                        int[] hashInts = AudioUtils.GetAudioHash(preparedPower, preparedFreqs);

                        string hash = "";
                        for (int i = 0; i < hashInts.Length; i++) hash += hashInts[i].ToString("00");
                        samplesList.Add(hash);
                    }

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
