using System.Collections.Generic;
using System;

namespace ScannerUI.Helpers
{
    /// <summary>
    /// Набор признаков, извлечённых для обнаруженного сигнала (плоская карта имя->число).
    /// </summary>
    public sealed class FeatureVector
    {
        public IDictionary<string, double> Numeric { get; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        // При необходимости можно добавить также категориальные/строковые признаки
        public IDictionary<string, string> Categorical { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
