namespace ScannerUI.Helpers
{
    /// <summary>
    /// Интервальная ссылка на участок входного IQ (индексы или длина). Это позволяет не копировать массивы.
    /// </summary>
    public readonly struct SampleRange
    {
        public int StartIndex { get; init; }
        public int Length { get; init; }
    }
}
