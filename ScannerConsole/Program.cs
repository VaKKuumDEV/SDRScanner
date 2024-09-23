using FftSharp;
using RtlSdrManager;
using RtlSdrManager.Types;
using System.Numerics;

var manager = RtlSdrDeviceManager.Instance;

Console.WriteLine("Count devices: " + manager.CountDevices);
Console.WriteLine("Devices: " + string.Join(", ", manager.Devices.Select(d => "(" + d.Value.Index + ") " + d.Value.Name)));

manager.OpenManagedDevice(0, "my-rtl-sdr");
manager["my-rtl-sdr"].CenterFrequency = new Frequency { MHz = 102 };
manager["my-rtl-sdr"].SampleRate = new Frequency { MHz = 0.25 };
manager["my-rtl-sdr"].TunerGainMode = TunerGainModes.AGC;
manager["my-rtl-sdr"].AGCMode = AGCModes.Enabled;
manager["my-rtl-sdr"].ResetDeviceBuffer();

var samples = manager["my-rtl-sdr"].ReadSamples(8192).Select(c => new System.Numerics.Complex(c.I, c.Q)).ToArray();
var power = FFT.Power(samples);

Console.WriteLine("Average power: " + power.Average());

manager.CloseAllManagedDevice();