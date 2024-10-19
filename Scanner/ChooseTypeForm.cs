namespace Scanner
{
    public partial class ChooseTypeForm : Form
    {
        public enum SdrTypes
        {
            USB,
            TCP,
        };

        public SdrTypes? ChoosenType { get; set; } = null;

        public ChooseTypeForm()
        {
            InitializeComponent();

            UsbButton.Click += new((sender, args) =>
            {
                ChoosenType = SdrTypes.USB;
                Close();
            });
            TcpButton.Click += new((sender, args) =>
            {
                ChoosenType = SdrTypes.TCP;
                Close();
            });
        }
    }
}
