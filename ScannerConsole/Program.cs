using SDRNet.HackRfOne;

const double SAMPLE_RATE = 20.48e6; // 20.48 Msps

HackRFDevice device = new();
device.SamplesAvailable += IO_SamplesAvailable;
device.Frequency = 2437u * 1000000;
device.SampleRate = (uint)SAMPLE_RATE;

device.Start();

Console.Read();
device.Stop();

static unsafe void IO_SamplesAvailable(object sender, SamplesAvailableEventArgs e)
{
    if (sender is HackRFDevice device)
    {
        
    }
}