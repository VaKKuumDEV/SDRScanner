namespace ScannerUI.Helpers
{
    /// <summary>
    /// Результат одного обнаруженного сигнала (парсируемый минимальный набор полей).
    /// </summary>
    public sealed class DetectionResult
    {
        public string DetectorName { get; init; } = string.Empty;
        public string? Label { get; init; } = null;
        public int? Channel { get; set; } = null;

        public override string ToString()
        {
            return $"{Label ?? "signal"} [{DetectorName}]" + (Channel != null ? $"[{Channel.Value}]" : string.Empty);
        }

        public override bool Equals(object? obj)
        {
            if (obj is DetectionResult result)
            {
                return result.Label == Label && result.DetectorName == DetectorName;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
