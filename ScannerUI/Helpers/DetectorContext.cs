using System.Collections.Generic;
using System;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Контекст вызова детектора — метаданные для блока IQ.
    /// </summary>
    public sealed class DetectorContext
    {
        /// <summary>Центральная частота в герцах для переданного IQ-блока.</summary>
        public double CenterFrequencyHz { get; init; }

        /// <summary>Частота дискретизации в Гц.</summary>
        public double SampleRateHz { get; init; }

        /// <summary>Временная метка начала блока (UTC/local по договоренности).</summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>Идентификатор буфера/сессии (опционально) — полезно для ссылок на raw IQ в хранилище.</summary>
        public string? BufferId { get; init; }

        /// <summary>Произвольные дополнительные параметры/флаги для детектора.</summary>
        public IDictionary<string, object?>? Options { get; init; }
    }
}
