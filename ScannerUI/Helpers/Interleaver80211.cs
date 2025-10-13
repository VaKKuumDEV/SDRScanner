using System;

namespace ScannerUI.Helpers
{
    public class Interleaver80211
    {
        private readonly int _Ncbps; // количество закодированных битов на символ
        private readonly int _Nbpsc; // количество битов на поднесущую
        private readonly int _s;

        public Interleaver80211(int Ncbps, int Nbpsc)
        {
            _Ncbps = Ncbps;
            _Nbpsc = Nbpsc;
            _s = Math.Max(_Nbpsc / 2, 1);
        }

        public double[] DeinterleaveSoft(double[] llr)
        {
            ArgumentNullException.ThrowIfNull(llr);
            if (llr.Length != _Ncbps)
                throw new ArgumentException($"llr.Length ({llr.Length}) != Ncbps ({_Ncbps})");

            double[] result = new double[_Ncbps];

            for (int j = 0; j < _Ncbps; j++)
            {
                // Первая перестановка
                int i = (_Ncbps / 16) * (j % 16) + (j / 16);

                // Вторая перестановка
                int k = _s * (i / _s) + (i + _Ncbps - (16 * i / _Ncbps)) % _s;

                result[k] = llr[j];
            }

            return result;
        }
    }
}
