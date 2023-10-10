using Microsoft.VisualBasic;
using NAudio.Wave;
using System.Numerics;

namespace Scanner.Audio
{
    public static class AudioUtils
    {
        public const double SAMPLE_RATE = 2e6;
        public const int POINTS = 10;
        public const int HASH_RATE = 100;

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

        public static (double[] power, double[] freqs) PrepareAudioData(double[] power, double[] freqs)
        {
            List<double> newPower = new(power);

            SortedDictionary<double, double> powerFreqs = new();
            for(int i = 0; i < freqs.Length - 1; i++)
            {
                double freq = Math.Floor(freqs[i]) + 1;
                if (!powerFreqs.ContainsKey(freq)) powerFreqs[freq] = 0d;
            }

            for(int i = 0; i < powerFreqs.Keys.Count; i++)
            {
                double freq = powerFreqs.Keys.ToArray()[i];
                int fromIndex = new List<double>(freqs).FindIndex(item => item >= (freq - 0.5));
                int toIndex = new List<double>(freqs).FindIndex(item => item >= (freq + 0.5));
                if (fromIndex == -1 || toIndex == -1) break;

                List<double> freqPowers = newPower.GetRange(fromIndex, toIndex - fromIndex);
                double averagedFreq = freqPowers.Average();
                powerFreqs[freq] = averagedFreq;
            }

            return (powerFreqs.Values.ToArray(), powerFreqs.Keys.ToArray());
        }

        public static int[] GetAudioHash(double[] powers, double[] freqs)
        {
            List<int> hash = new();
            SortedDictionary<double, double> newPowerData = new();
            for (int i = 1; i < freqs[^1] + 1; i++) newPowerData[i] = 0;

            for(int i = 0; i < freqs.Length; i++) newPowerData[freqs[i]] = powers[i];
            for(int i = 0; i < newPowerData.Keys.Count; i += HASH_RATE)
            {
                int rangeCount = (newPowerData.Keys.Count - i < HASH_RATE) ? newPowerData.Keys.Count - i : HASH_RATE;
                List<double> sampleData = newPowerData.Values.ToList().GetRange(i, rangeCount);

                int maxIndex = 0;
                for (int j = 0; j < sampleData.Count; j++) if (sampleData[j] > sampleData[maxIndex]) maxIndex = j;
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
    }
}
