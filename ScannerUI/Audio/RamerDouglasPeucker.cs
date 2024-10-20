using System.Collections.Generic;

namespace ScannerUI.Audio
{
    /*
     * Sources:
     * https://www.codeproject.com/Articles/18936/A-C-Implementation-of-Douglas-Peucker-Line-Approxi
     * https://codereview.stackexchange.com/questions/29002/ramer-douglas-peucker-algorithm
     * Optimisations:
     *  - Do not use Sqrt function
     *  - Use unsafe code
     *  - Avoid duplicate computations in loop
     */
    public static class RamerDouglasPeucker
    {
        /// <summary>
        /// Uses the Ramer Douglas Peucker algorithm to reduce the number of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public unsafe static AudioUtils.Point[] Reduce(AudioUtils.Point* points, double tolerance, int length)
        {
            if (length < 3) return [];
            if (double.IsInfinity(tolerance) || double.IsNaN(tolerance)) return [];
            tolerance *= tolerance;
            if (tolerance <= float.Epsilon) return [];

            int firstIndex = 0;
            int lastIndex = length - 1;
            List<int> indexesToKeep =
            [
                // Add the first and last index to the keepers
                firstIndex,
                lastIndex,
            ];

            // The first and the last point cannot be the same
            while (points[firstIndex].Equals(points[lastIndex]))
            {
                lastIndex--;
            }

            Reduce(points, length, firstIndex, lastIndex, tolerance, ref indexesToKeep);

            int l = indexesToKeep.Count;
            AudioUtils.Point[] returnPoints = new AudioUtils.Point[l];
            indexesToKeep.Sort();

            fixed (AudioUtils.Point* result = returnPoints)
            {
                for (int i = 0; i < l; ++i) *(result + i) = *(points + indexesToKeep[i]);
            }

            return returnPoints;
        }

        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstIndex">The first point's index.</param>
        /// <param name="lastIndex">The last point's index.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="indexesToKeep">The points' index to keep.</param>
        private unsafe static void Reduce(AudioUtils.Point* points, int length, int firstIndex, int lastIndex, double tolerance, ref List<int> indexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            AudioUtils.Point point1 = *(points + firstIndex);
            AudioUtils.Point point2 = *(points + lastIndex);
            double distXY = point1.X * point2.Y - point2.X * point1.Y;
            double distX = point2.X - point1.X;
            double distY = point1.Y - point2.Y;
            double bottom = distX * distX + distY * distY;

            for (int i = firstIndex; i < lastIndex; i++)
            {
                // Perpendicular Distance
                AudioUtils.Point point = *(points + i);
                double area = distXY + distX * point.Y + distY * point.X;
                double distance = (area / bottom) * area;

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = i;
                }
            }

            if (maxDistance > tolerance) // && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                indexesToKeep.Add(indexFarthest);
                Reduce(points, length, firstIndex, indexFarthest, tolerance, ref indexesToKeep);
                Reduce(points, length, indexFarthest, lastIndex, tolerance, ref indexesToKeep);
            }
        }
    }
}

