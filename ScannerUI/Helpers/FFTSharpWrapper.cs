namespace ScannerUI.Helpers
{
    /// <summary>
    /// Обертка для FFTSharp — преобразует SDRNet.Radio.Complex <-> System.Numerics.Complex
    /// </summary>
    public static class FFTSharpWrapper
    {
        public static SDRNet.Radio.Complex[] FFT(SDRNet.Radio.Complex[] data)
        {
            var sys = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                sys[i] = new System.Numerics.Complex(data[i].Real, data[i].Imag);

            FftSharp.FFT.Forward(sys);

            var result = new SDRNet.Radio.Complex[data.Length];
            for (int i = 0; i < sys.Length; i++)
                result[i] = new SDRNet.Radio.Complex((float)sys[i].Real, (float)sys[i].Imaginary);

            return result;
        }
    }
}
