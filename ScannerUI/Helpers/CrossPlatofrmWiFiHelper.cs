using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScannerUI.Helpers
{
    public record class WiFiSignal(string Ssid, int Channel);

    public static class CrossPlatofrmWiFiHelper
    {
        public static async Task<IEnumerable<WiFiSignal>> EnumerateWifi()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var networks = new List<WiFiSignal>();

                var output = RunCommand("sudo", "iw dev wlan0 scan");
                string[] lines = output.Split('\n');

                string? currentSsid = null;
                foreach (var line in lines)
                {
                    if (line.Contains("SSID:"))
                        currentSsid = line.Split(["SSID:"], 2, StringSplitOptions.None)[1].Trim();

                    else if (currentSsid != null && line.Contains("primary channel:"))
                    {
                        if (int.TryParse(line.Split(':')[1].Trim(), out int ch))
                        {
                            networks.Add(new(currentSsid, ch));
                            currentSsid = null;
                        }
                    }
                }

                return networks;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await ManagedNativeWifi.NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
                var networks = ManagedNativeWifi.NativeWifi.EnumerateBssNetworks();

                return networks.Select(n => new WiFiSignal(n.Ssid.ToString(), n.Channel));
            }

            return [];
        }

        private static string RunCommand(string cmd, string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }
}
