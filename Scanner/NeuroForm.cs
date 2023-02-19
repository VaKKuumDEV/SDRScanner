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

namespace Scanner
{
    public partial class NeuroForm : Form
    {
        public NeuroForm()
        {
            InitializeComponent();

            SignalPlot.Plot.Title("БПФ без фильтрации");
            SignalPlot.Plot.XLabel("Частота (Гц)");
            SignalPlot.Plot.YLabel("Мощность (дБ)");

            PreparedSignalPlot.Plot.Title("БПФ с фильтром");
            PreparedSignalPlot.Plot.XLabel("Частота (Гц)");
            PreparedSignalPlot.Plot.YLabel("Мощность (дБ)");
        }

        private void OpenSignalButton_Click(object sender, EventArgs e)
        {
            OpenSignalDialog.ShowDialog(this);
        }

        private async void OpenSignalDialog_FileOk(object sender, CancelEventArgs e)
        {
            ContentPanel.Enabled = false;
            Cursor = Cursors.WaitCursor;

            await Task.Run(() =>
            {
                try
                {
                    (double[] audio, int sampleRate) = AudioUtils.ReadAudioFile(new(OpenSignalDialog.FileName));
                    (double[] power, double[] freqs) = AudioUtils.MakeFFT(audio, sampleRate);
                    (double[] preparedPower, double[] preparedFreqs) = AudioUtils.PrepareAudioData(power, freqs);

                    BeginInvoke(() =>
                    {
                        SignalPlot.Plot.Clear();
                        PreparedSignalPlot.Plot.Clear();

                        SignalPlot.Plot.AddSignalXY(freqs, power);
                        PreparedSignalPlot.Plot.AddSignalXY(preparedFreqs, preparedPower, Color.Green);

                        SignalPlot.Refresh();
                        PreparedSignalPlot.Refresh();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            ContentPanel.Enabled = true;
            Cursor = Cursors.Arrow;
        }
    }
}
