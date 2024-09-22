using RtlSdrManager;
using RtlSdrManager.Types;

var manager = RtlSdrDeviceManager.Instance;

Console.WriteLine("Count devices: " + manager.CountDevices);
Console.WriteLine("Devices: " + string.Join(", ", manager.Devices.Select(d => "(" + d.Value.Index + ") " + d.Value.Name)));

manager.OpenManagedDevice(0, "my-rtl-sdr");
manager["my-rtl-sdr"].CenterFrequency = new Frequency { MHz = 102 };
manager["my-rtl-sdr"].SampleRate = new Frequency { MHz = 2 };
manager["my-rtl-sdr"].TunerGainMode = TunerGainModes.AGC;
manager["my-rtl-sdr"].AGCMode = AGCModes.Enabled;
manager["my-rtl-sdr"].ResetDeviceBuffer();

var samples = manager["my-rtl-sdr"].ReadSamples(256);

Console.WriteLine("Read samples: " + samples.Count);
Console.WriteLine("Samples (first 20):");
for (var i = 0; i < 20; i++)
{
    Console.WriteLine($"  {i + 1:00}: {samples[i]}");
}

manager.CloseAllManagedDevice();