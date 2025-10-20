using InTheHand.Bluetooth;
using InTheHand.Net.Sockets;

await ManagedNativeWifi.NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
var networks = ManagedNativeWifi.NativeWifi.EnumerateBssNetworks();

Console.WriteLine("WiFi:");
foreach (var network in networks)
{
    Console.WriteLine(network.Ssid.ToString() + ": " + network.Channel);
}

Console.WriteLine("");
Console.WriteLine("Bluetooth:");

BluetoothClient client = new();
var devices = client.DiscoverDevices().ToList();

foreach (var device in devices)
{
    Console.WriteLine(device.DeviceName);
}