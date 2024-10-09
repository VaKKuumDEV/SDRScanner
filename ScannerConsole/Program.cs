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

while (true)
{
    var samples = manager["my-rtl-sdr"].ReadSamples(8192).Select(s => new System.Numerics.Complex(s.I, s.Q)).ToList();
    File.AppendAllLines("output.txt", samples.Select(s => s.Real + ":" + s.Imaginary));

    var power = FFT.Power([.. samples]);
    Console.WriteLine(power.Average());
}

Console.Read();
manager.CloseAllManagedDevice();