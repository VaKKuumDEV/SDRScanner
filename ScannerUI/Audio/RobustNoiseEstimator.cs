using System;
using System.Collections.Generic;
using System.Linq;

namespace ScannerUI.Audio
{
    public static class RobustNoiseEstimator
    {
        /// <summary>
        /// Оценивает уровень шума в спектре, игнорируя верхние выбросы (сигналы).
        /// Цель: получить значение, близкое к уровню шума без сигналов (~49 дБ).
        /// </summary>
        /// <param name="spectrumDb">Спектр в дБ</param>
        /// <param name="blockSize">Размер блока (рекомендуется 64–256)</param>
        /// <param name="clipTopPercent">Процент верхних значений, которые игнорировать в каждом блоке (по умолчанию 10%)</param>
        /// <returns>Оценка уровня шума</returns>
        public unsafe static float EstimateNoiseLevel(float* spectrumDb, int len, int blockSize = 128, double clipTopPercent = 10.0)
        {
            if (spectrumDb == null || len == 0)
                throw new ArgumentException("Спектр не может быть пустым.");

            if (blockSize <= 0)
                throw new ArgumentException("Размер блока должен быть положительным.");

            if (clipTopPercent < 0 || clipTopPercent >= 100)
                throw new ArgumentException("clipTopPercent должен быть в диапазоне [0, 100)");

            var blockMedians = new List<float>();

            for (int i = 0; i < len; i += blockSize)
            {
                int length = Math.Min(blockSize, len - i);
                var block = new float[length];
                for (int j = 0; j < length; j++)
                {
                    block[j] = spectrumDb[i + j];
                }

                // Сортируем блок
                var sortedBlock = block.OrderBy(x => x).ToArray();

                // Отсекаем верхний процент
                int clipCount = (int)Math.Ceiling(length * clipTopPercent / 100.0);
                int validLength = length - clipCount;

                if (validLength <= 0)
                    continue; // Если всё отсечено — пропускаем блок

                // Берём медиану от оставшихся значений
                var trimmedBlock = sortedBlock.Take(validLength).ToArray();
                float median = Median(trimmedBlock);

                blockMedians.Add(median);
            }

            if (blockMedians.Count == 0)
                throw new InvalidOperationException("Не удалось оценить уровень шума — все блоки были отсечены.");

            return Median([.. blockMedians]);
        }

        private static float Median(float[] values)
        {
            var sorted = values.OrderBy(x => x).ToArray();
            int n = sorted.Length;
            return n % 2 == 0
                ? (sorted[n / 2 - 1] + sorted[n / 2]) / 2f
                : sorted[n / 2];
        }
    }
}
