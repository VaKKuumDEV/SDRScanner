using System;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Helpers
{
    public static class BluetoothChannelHelper
    {
        private static readonly Dictionary<int, int> BluetoothGHz = new()
        {
            { 2402, 37 }, { 2426, 38 }, { 2480, 39 },
        };

        public static int FreqToChannel(double freqMHz)
        {
            int freq = (int)Math.Round(freqMHz);

            if (BluetoothGHz.TryGetValue(freq, out int ch24))
                return ch24;

            return 0;
        }

        public static int[] GetChannelFreqs() => [.. BluetoothGHz.Keys];

        public static bool IsBluetoothFrequency(double frequency) => BluetoothGHz.ContainsKey(Convert.ToInt32(frequency));
    }
}
