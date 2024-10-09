using FftSharp;
using RtlSdrManager;
using RtlSdrManager.Types;

var manager = RtlSdrDeviceManager.Instance;

Console.WriteLine("Count devices: " + manager.CountDevices);
Console.WriteLine("Devices: " + string.Join(", ", manager.Devices.Select(d => "(" + d.Value.Index + ") " + d.Value.Name)));

manager.OpenManagedDevice(0, "my-rtl-sdr");
manager["my-rtl-sdr"].CenterFrequency = new Frequency { MHz = 433 };
manager["my-rtl-sdr"].SampleRate = new Frequency { MHz = 2 };
manager["my-rtl-sdr"].TunerGainMode = TunerGainModes.AGC;
manager["my-rtl-sdr"].AGCMode = AGCModes.Enabled;
manager["my-rtl-sdr"].ResetDeviceBuffer();

manager["my-rtl-sdr"].StartReadSamplesAsync();
manager["my-rtl-sdr"].SamplesAvailable += new((sender, args) =>
{
    var samples = manager["my-rtl-sdr"].GetSamplesFromAsyncBuffer(args.SampleCount);
});

Console.Read();
manager.CloseAllManagedDevice();