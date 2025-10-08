using ScannerUI.Helpers;
using SDRNet.Radio;
using System.Threading;

namespace ScannerUI.Detector
{
    /// <summary>
    /// Общий интерфейс для детекторов. Метод Detect возвращает массив найденных сигналов в переданном IQ-блоке.
    /// </summary>
    public interface IDetector
    {
        /// <summary>
        /// Человеко‑читаемое имя детектора (например, "SpectralPeakDetector", "BleEnergyDetector").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Выполнить обнаружение на блоке IQ сэмплов.
        /// </summary>
        /// <param name="iq">
        /// Массив комплексных IQ-сэмплов. Реализация не должна мутировать входной массив.
        /// </param>
        /// <param name="context">
        /// Метаданные блока (центр. частота, Fs, timestamp и т.д.)
        /// </param>
        /// <param name="cancellation">
        /// Токен отмены — полезен для прерывания тяжёлых вычислений при остановке/переключении канала.
        /// </param>
        /// <returns>
        /// Массив результатов обнаружения (может быть пустым). Каждый элемент описывает один локализованный в частоте/времени объект.
        /// </returns>
        DetectionResult[] Detect(Complex[] iq, DetectorContext context, CancellationToken cancellation = default);
    }
}
