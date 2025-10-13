using System.Collections.Generic;
using System;

namespace ScannerUI.Audio
{
    public static class MathUtils
    {
        public static IEnumerable<double> Interpolate(double[] inputs, int count)
        {
            double maxCountForIndexCalculation = count - 1;
            for (int index = 0; index < count; index++)
            {
                double floatingIndex = (index / maxCountForIndexCalculation) * (inputs.Length - 1);

                int baseIndex = (int)floatingIndex;
                double fraction = floatingIndex - baseIndex;

                if (Math.Abs(fraction) < 1e-5)
                    yield return inputs[baseIndex];
                else
                {
                    double delta = inputs[baseIndex + 1] - inputs[baseIndex];
                    yield return inputs[baseIndex] + fraction * delta;
                }
            }
        }
    }
}
