namespace Scanner
{
    public partial class AddSignalForm : Form
    {
        public string? NewSignalName { get; set; } = null;

        public AddSignalForm()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (SignalNameBox.Text.Trim().Length > 0)
            {
                NewSignalName = SignalNameBox.Text;
                Close();
            }
        }
    }
}
