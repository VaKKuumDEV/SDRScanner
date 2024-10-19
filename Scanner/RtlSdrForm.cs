using SDRSharp.RTLSDR;

namespace Scanner
{
    public partial class RtlSdrForm : Form
    {
        public RtlSdrIO? RtlSdrIO { get; set; } = null;
        private List<DeviceDisplay> DevicesList { get; } = [];

        public RtlSdrForm()
        {
            InitializeComponent();

            DevicesBox.SelectedIndexChanged += new((sender, args) =>
            {
                int index = DevicesBox.SelectedIndex;
                if (index != -1 && index < DevicesList.Count)
                {
                    DeviceDisplay device = DevicesList[index];
                    RtlSdrIO = new RtlSdrIO();
                    RtlSdrIO.SelectDevice(device.Index);

                    GainBox.Items.Clear();
                    if (RtlSdrIO.Device != null)
                    {
                        foreach (int gain in RtlSdrIO.Device.SupportedGains) GainBox.Items.Add(gain.ToString() + " дБ");
                        if (GainBox.Items.Count > 0) GainBox.SelectedIndex = 0;
                    }
                }
            });

            GainBox.SelectedIndexChanged += new((sender, args) =>
            {
                int index = GainBox.SelectedIndex;
                if (RtlSdrIO != null && RtlSdrIO.Device != null)
                {
                    int gain = RtlSdrIO.Device.SupportedGains[index];
                    RtlSdrIO.Device.Gain = gain;
                }
            });

            ControlButton.Click += new((sender, args) =>
            {
                Close();
            });

            LoadDevicesList();
        }

        private void LoadDevicesList()
        {
            DeviceDisplay[] devices = DeviceDisplay.GetActiveDevices();
            DevicesList.Clear();
            DevicesList.AddRange(devices);

            DevicesBox.Items.Clear();
            foreach (DeviceDisplay device in DevicesList) DevicesBox.Items.Add("(#" + device.Index + ") " + device.Name);

            if (DevicesList.Count > 0) DevicesBox.SelectedIndex = 0;
        }
    }
}
