using SDRNet.Radio;
using SDRNet.HackRfOne;

DeviceDisplay[] devices = DeviceDisplay.GetActiveDevices();
if (devices.Length > 0)
{
    Console.WriteLine("Devices:");
    for (int i = 0; i < devices.Length; i++) Console.WriteLine(string.Format("Device {0}: {1}", devices[i].Index, devices[i].Name));

    Console.WriteLine();
    Console.Write("Enter index: ");

    int deviceIndex = int.Parse(Console.ReadLine() ?? "0");
    HackRFDevice device = new();
    device.SamplesAvailable += IO_SamplesAvailable;
    device.Frequency = 433950000u;

    device.Start();

    Console.Read();
    device.Stop();
}
else
{
    Console.WriteLine("No devices found");
}

static unsafe void IO_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
{
    if (sender is HackRFDevice device)
    {
        Fourier.ForwardTransform(e.Buffer, e.Length);

        float[] power = new float[e.Length];
        float[] simpleAveraged = new float[e.Length];
        fixed (float* srcPower = power)
        {
            Fourier.SpectrumPower(e.Buffer, srcPower, e.Length, device.LNAGain);
        }

        Console.WriteLine("Average: " + power.Average());
    }
}