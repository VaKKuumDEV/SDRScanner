using System;
using System.Collections.Generic;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Утилиты для преобразования между Wi-Fi частотой и номером канала.
    /// Поддерживает 2.4 ГГц и 5 ГГц диапазоны, включая HT40/VHT80.
    /// </summary>
    public static class WiFiChannelHelper
    {
        /// <summary>
        /// Возвращает центральную частоту канала (в МГц)
        /// </summary>
        public static double ChannelToFreqMHz(int channel)
        {
            if (channel >= 1 && channel <= 13)
                return 2407 + 5 * channel; // 2.4 GHz (20 MHz)
            else if (channel == 14)
                return 2484; // только Япония
            else if (channel >= 36 && channel <= 165)
                return 5000 + 5 * channel; // 5 GHz диапазон
            else if (channel >= 183 && channel <= 196)
                return 4000 + 5 * channel; // редкие 4.9 GHz (регионально)
            else
                throw new ArgumentOutOfRangeException(nameof(channel), $"Неизвестный канал {channel}");
        }

        /// <summary>
        /// Возвращает центральную частоту канала в Гц
        /// </summary>
        public static double ChannelToFreqHz(int channel) => ChannelToFreqMHz(channel) * 1e6;

        /// <summary>
        /// Возвращает ближайший номер канала для данной частоты (в МГц)
        /// </summary>
        public static int FreqToChannel(double freqMHz)
        {
            // 2.4 GHz
            if (freqMHz >= 2412 && freqMHz <= 2472)
                return (int)Math.Round((freqMHz - 2407) / 5);
            if (Math.Abs(freqMHz - 2484) < 2)
                return 14;

            // 5 GHz
            if (freqMHz >= 5180 && freqMHz <= 5865)
                return (int)Math.Round((freqMHz - 5000) / 5);

            // 4.9 GHz (редкий)
            if (freqMHz >= 4915 && freqMHz <= 4995)
                return (int)Math.Round((freqMHz - 4000) / 5);

            throw new ArgumentOutOfRangeException(nameof(freqMHz), $"Частота {freqMHz:F1} МГц не принадлежит Wi-Fi диапазонам");
        }

        /// <summary>
        /// Возвращает список допустимых каналов для заданной полосы (MHz)
        /// </summary>
        public static List<int> GetChannelsForBand(double freqMHz)
        {
            if (freqMHz < 3000)
                return [1, 6, 11]; // типично для 2.4 GHz
            else
                return [36, 40, 44, 48, 52, 56, 60, 64, 100, 104, 108, 112, 116, 120, 124, 128, 132, 136, 140, 149, 153, 157, 161, 165];
        }

        /// <summary>
        /// Возвращает смещение частоты (в Гц) для вторичного канала в HT40/80 режимах
        /// </summary>
        public static double GetSecondaryOffsetHz(int channel, int bandwidthMHz)
        {
            if (bandwidthMHz == 20) return 0;
            if (bandwidthMHz == 40) return 20e6;
            if (bandwidthMHz == 80) return 40e6;
            throw new ArgumentException("bandwidthMHz должен быть 20, 40 или 80");
        }

        /// <summary>
        /// Проверяет, принадлежит ли частота диапазону Wi-Fi
        /// </summary>
        public static bool IsWiFiFrequency(double freqMHz)
        {
            return (freqMHz >= 2400 && freqMHz <= 2500) ||
                   (freqMHz >= 4900 && freqMHz <= 5900);
        }

        /// <summary>
        /// Возвращает описание диапазона по частоте
        /// </summary>
        public static string BandDescription(double freqMHz)
        {
            if (freqMHz >= 2400 && freqMHz < 2500)
                return "2.4 GHz ISM";
            if (freqMHz >= 4900 && freqMHz < 5900)
                return "5 GHz UNII";
            return "Неизвестный диапазон";
        }
    }
}
