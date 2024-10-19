using SDRSharp.RTLTCP;

namespace Scanner
{
    public partial class RtlTcpForm : Form
    {
        public RtlTcpIO? RtlTcpIO { get; set; } = null;

        public RtlTcpForm()
        {
            InitializeComponent();

            ControlButton.Click += new((sender, args) =>
            {
                RtlTcpIO = new(IpBox.Text, Convert.ToInt32(PortBox.Text));
                Close();
            });
        }
    }
}
