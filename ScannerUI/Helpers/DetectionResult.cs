namespace ScannerUI.Helpers
{
    /// <summary>
    /// Результат одного обнаруженного сигнала (парсируемый минимальный набор полей).
    /// </summary>
    public sealed class DetectionResult
    {
        /// <summary>Имя/тип детектора, который сгенерировал этот результат.</summary>
        public string DetectorName { get; init; } = string.Empty;

        /// <summary>Опциональная семантическая метка (пример: "BLE_adv", "WiFi_probe", "OOK_433").</summary>
        public string? Label { get; init; }
        /// <summary>Человеко‑читаемая сводка (короткая).</summary>
        public override string ToString()
        {
            return $"{Label ?? "signal"} [{DetectorName}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is DetectionResult result)
            {
                return result.Label == Label && result.DetectorName == DetectorName;
            }

            return base.Equals(obj);
        }
    }
}
