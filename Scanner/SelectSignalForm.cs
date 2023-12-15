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
    public partial class SelectSignalForm : Form
    {
        private SignalsMap Map { get; }
        public string? SelectedSignal { get; set; } = null;

        public SelectSignalForm()
        {
            Map = new(new DirectoryInfo(Environment.CurrentDirectory + "/Samples.json").FullName);
            InitializeComponent();

            foreach (var kv in Map.Map) SignalsBox.Items.Add(kv.Key);
        }

        private void AbbSignalButon_Click(object sender, EventArgs e)
        {
            AddSignalForm form = new();
            form.ShowDialog(this);

            if(form.NewSignalName != null && !Map.Map.ContainsKey(form.NewSignalName))
            {
                Map.Map[form.NewSignalName] = new();
                Map.Save();

                SelectedSignal = form.NewSignalName;
                Close();
            }
        }

        private void SelectSignalForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            int selectedIndex = SignalsBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < Map.Map.Count) SelectedSignal = Map.Map.ToList()[selectedIndex].Key;
        }
    }
}
