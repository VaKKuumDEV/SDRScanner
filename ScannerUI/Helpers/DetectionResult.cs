using System.Collections.Generic;
using System;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Результат одного обнаруженного сигнала (парсируемый минимальный набор полей).
    /// </summary>
    public sealed class DetectionResult
    {
        /// <summary>Уникальный идентификатор обнаружения (в пределах процесса).</summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>Имя/тип детектора, который сгенерировал этот результат.</summary>
        public string DetectorName { get; init; } = string.Empty;

        /// <summary>Опциональная семантическая метка (пример: "BLE_adv", "WiFi_probe", "OOK_433").</summary>
        public string? Label { get; init; }

        /// <summary>Вероятность/доверие в диапазоне [0..1].</summary>
        public double Confidence { get; set; }

        /// <summary>Оценка мощности/уровня сигнала (dBFS или относительный dB), по договоренности в системе.</summary>
        public double? EstimatedPowerDb { get; init; }

        /// <summary>Оценочная пиковая частота сигнала (Hz, абсолютная, включая центр блока).</summary>
        public double? PeakFrequencyHz { get; init; }

        /// <summary>Оценочная ширина полосы сигнала (Hz).</summary>
        public double? EstimatedBandwidthHz { get; init; }

        /// <summary>Оценка SNR (дБ), если детектор её вычисляет.</summary>
        public double? EstimatedSnrDb { get; init; }

        /// <summary>Временная метка начала детектируемого события (если релевантно).</summary>
        public DateTimeOffset? StartTime { get; init; }

        /// <summary>Длительность события в секундах (если релевантно).</summary>
        public double? DurationSeconds { get; init; }

        /// <summary>Ссылка на диапазон сэмплов во входном буфере (если применимо).</summary>
        public SampleRange? SamplesRange { get; init; }

        /// <summary>Идентификатор или ссылка на raw IQ хранилище (если сырые данные сохранены отдельно).</summary>
        public string? RawBufferRef { get; init; }

        /// <summary>Дополнительные извлечённые признаки (см. FeatureVector).</summary>
        public FeatureVector Features { get; init; } = new FeatureVector();

        /// <summary>Произвольные ключ‑значение метаданные (для специфичных нужд детектора).</summary>
        public IDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();

        /// <summary>Человеко‑читаемая сводка (короткая).</summary>
        public override string ToString()
        {
            return $"{Label ?? "signal"} [{DetectorName}] conf={Confidence:F2} p={EstimatedPowerDb?.ToString("F1") ?? "n/a"} d:{EstimatedBandwidthHz?.ToString("F0") ?? "n/a"}Hz f:{PeakFrequencyHz?.ToString("F0") ?? "n/a"}Hz";
        }
    }
}
