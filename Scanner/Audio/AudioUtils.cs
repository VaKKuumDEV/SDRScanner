using NAudio.Wave;

namespace Scanner.Audio
{
    public static class AudioUtils
    {
        public const double SAMPLE_RATE = 2e6;
        public const int POINTS = 10;
        public const int HASH_RATE = 100;

        public readonly struct Point
        {
            public double X { get; }
            public double Y { get; }
            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        };

        public static (double[] audio, int sampleRate, int bytesPerSample, int totalTime) ReadAudioFile(FileInfo file, double multiplier = 16__000)
        {
            using var afr = new AudioFileReader(file.FullName);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int totalTime = (int)afr.TotalTime.TotalSeconds;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelsCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelsCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0) audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate, bytesPerSample, totalTime);
        }

        public static KeyValuePair<double[], double[]>[] MakeFFT(double[] audio, int sampleRate, int totalTime)
        {
            if (totalTime < 3) throw new Exception("Audio length must be more or equals then 3 second");
            int samples = totalTime / 3; //время одного сэмпла сигнала
            int sampleLength = audio.Length / samples;
            if (!IsPowerOfTwo(sampleLength)) sampleLength = (int)Math.Pow(2, Math.Floor(Math.Log2(sampleLength)));
            samples = audio.Length / sampleLength;

            List<KeyValuePair<double[], double[]>> data = new();
            for(int i = 0; i < samples; i++)
            {
                double[] sampleData = new FftSharp.Windows.Hanning().Apply(new List<double>(audio).GetRange(i * sampleLength, sampleLength).ToArray());
                if (!IsPowerOfTwo(sampleData.Length)) continue;

                System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(sampleData);

                double[] samplePower = FftSharp.FFT.Power(spectrum);
                double[] sampleFreqs = FftSharp.FFT.FrequencyScale(samplePower.Length, sampleRate);
                data.Add(new(samplePower, sampleFreqs));
            }

            return data.ToArray();
        }

        public static (double[] power, double[] freqs) PrepareAudioData(double[] power, double[] freqs, double step = 1)
        {
            SortedDictionary<double, double> powerFreqs = new();
            if (step <= 1)
            {
                for (int i = 0; i < freqs.Length - 1; i++)
                {
                    double freq = Math.Floor(freqs[i]) + 1;
                    if (!powerFreqs.ContainsKey(freq)) powerFreqs[freq] = 0d;
                }

                for (int i = 0; i < powerFreqs.Keys.Count; i++)
                {
                    double freq = powerFreqs.Keys.ToArray()[i];
                    int fromIndex = new List<double>(freqs).FindIndex(item => item >= (freq - (step / 2)) && item <= (freq + (step / 2)));
                    int toIndex = new List<double>(freqs).FindLastIndex(item => item >= (freq - (step / 2)) && item <= (freq + (step / 2)));
                    if (fromIndex == -1) fromIndex = 0;
                    if (toIndex == -1) toIndex = 0;

                    List<double> freqPowers = new List<double>(power).GetRange(fromIndex, Math.Max(1, toIndex - fromIndex + 1));
                    double averagedFreq = freqPowers.Average();
                    powerFreqs[freq] = averagedFreq;
                }
            }
            else
            {
                for (int i = 0; i < power.Length; i++)
                {
                    powerFreqs[Math.Floor(freqs[i])] = power[i];
                }
            }

            return (powerFreqs.Values.ToArray(), powerFreqs.Keys.ToArray());
        }

        public static int[] GetAudioHash(double[] powers, int hashRate = HASH_RATE)
        {
            List<int> hash = new();

            for(int leftIndex = 0; leftIndex < powers.Length; leftIndex += hashRate)
            {
                int maxIndex = 0;
                for(int i = 0; i < hashRate; i++)
                {
                    if (leftIndex + i >= powers.Length) break;
                    if (powers[leftIndex + i] > powers[leftIndex + maxIndex]) maxIndex = i;
                }

                hash.Add(maxIndex);
            }

            return hash.ToArray();
        }

        public static IEnumerable<string> ChunkSplit(string str, int chunkSize)
        {
            return Enumerable.Range(0, (int)Math.Ceiling(str.Length / (float)chunkSize)).Select(i => str.Substring(i * chunkSize, Math.Min(str.Length - (i * chunkSize), chunkSize)));
        }

        public static double? CompareHashes(string firstHash, string secondHash)
        {
            if (firstHash.Length != secondHash.Length) return null;

            List<int> firstInt = new List<string>(ChunkSplit(firstHash, 2)).ConvertAll(i => Convert.ToInt32(i));
            List<int> secondInt = new List<string>(ChunkSplit(secondHash, 2)).ConvertAll(i => Convert.ToInt32(i));

            double avg1 = firstInt.Average();
            double avg2 = secondInt.Average();

            double sum1 = firstInt.Zip(secondInt, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            double sumSqr1 = firstInt.Sum(x => Math.Pow(x - avg1, 2.0));
            double sumSqr2 = secondInt.Sum(y => Math.Pow(y - avg2, 2.0));

            double correl = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);
            return correl;
        }

        public static bool IsPowerOfTwo(int x)
        {
            return ((x & (x - 1)) == 0) && (x > 0);
        }

        public unsafe static void WriteToWav(string filename, MemoryStream iStream, MemoryStream qStream)
        {
            FileInfo file = new(Environment.CurrentDirectory + "/" + filename + ".wav");

            RawSourceWaveStream iComponent = new(iStream, new WaveFormat(44100, 16, 1));
            RawSourceWaveStream qComponent = new(qStream, new WaveFormat(44100, 16, 1));

            MultiplexingWaveProvider waveProvider = new(new IWaveProvider[] { iComponent, qComponent }, 2);

            WaveFileWriter.CreateWaveFile(file.FullName, waveProvider);
        }

        private static double DistanceToSegment(Point p, Point start, Point end)
        {
            var m1 = (end.Y - start.Y) / ((double)(end.X - start.X));
            var c1 = start.Y - m1 * start.X;
            double interPointX, interPointY;
            if (m1 == 0)
            {
                interPointX = p.X;
                interPointY = c1;

            }
            else
            {
                var m2 = -1 / m1;
                var c2 = p.Y - m2 * p.X;
                interPointX = (c1 - c2) / (m2 - m1);
                interPointY = m2 * interPointX + c2;
            }
            return Math.Sqrt(Math.Pow(p.X - interPointX, 2) + Math.Pow(p.Y - interPointY, 2));
        }

        public static void DouglasPeucker(IList<Point> PointList, double epsilon, ref List<Point> filteredPoints)
        {
            var dmax = 0d;
            int index = 0;
            int length = PointList.Count;
            for (int i = 1; i < length - 1; i++)
            {
                var d = DistanceToSegment(PointList[i], PointList[0], PointList[length - 1]);
                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            if (dmax > epsilon)
            {
                filteredPoints.Add(PointList[0]);
                filteredPoints.Add(PointList[index]);
                filteredPoints.Add(PointList[length - 1]);
                DouglasPeucker(PointList.Take(index + 1).ToList(), epsilon, ref filteredPoints);
                DouglasPeucker(PointList.Skip(index + 1).Take(PointList.Count - index - 1).ToList(), epsilon, ref filteredPoints);
            }
        }
    }
}
