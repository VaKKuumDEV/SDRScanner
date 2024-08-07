using static Scanner.Audio.AudioUtils;

namespace Scanner.Audio
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
        public static AudioUtils.Point[] Reduce(AudioUtils.Point[] points, double tolerance)
        {
            if (points.Length < 3) return points;
            if (double.IsInfinity(tolerance) || double.IsNaN(tolerance)) return points;
            tolerance *= tolerance;
            if (tolerance <= float.Epsilon) return points;

            int firstIndex = 0;
            int lastIndex = points.Length - 1;
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

            Reduce(points, firstIndex, lastIndex, tolerance, ref indexesToKeep);

            int l = indexesToKeep.Count;
            AudioUtils.Point[] returnPoints = new AudioUtils.Point[l];
            indexesToKeep.Sort();

            unsafe
            {
                fixed (AudioUtils.Point* ptr = points, result = returnPoints)
                {
                    for (int i = 0; i < l; ++i)
                        *(result + i) = *(ptr + indexesToKeep[i]);
                }
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
        private static void Reduce(AudioUtils.Point[] points, int firstIndex, int lastIndex, double tolerance, ref List<int> indexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            unsafe
            {
                fixed (AudioUtils.Point* samples = points)
                {
                    AudioUtils.Point point1 = *(samples + firstIndex);
                    AudioUtils.Point point2 = *(samples + lastIndex);
                    double distXY = point1.X * point2.Y - point2.X * point1.Y;
                    double distX = point2.X - point1.X;
                    double distY = point1.Y - point2.Y;
                    double bottom = distX * distX + distY * distY;

                    for (int i = firstIndex; i < lastIndex; i++)
                    {
                        // Perpendicular Distance
                        AudioUtils.Point point = *(samples + i);
                        double area = distXY + distX * point.Y + distY * point.X;
                        double distance = (area / bottom) * area;

                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            indexFarthest = i;
                        }
                    }
                }
            }

            if (maxDistance > tolerance) // && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                indexesToKeep.Add(indexFarthest);
                Reduce(points, firstIndex, indexFarthest, tolerance, ref indexesToKeep);
                Reduce(points, indexFarthest, lastIndex, tolerance, ref indexesToKeep);
            }
        }
    }
}

