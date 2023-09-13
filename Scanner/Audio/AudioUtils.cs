using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (totalTime < 10) throw new Exception("Audio length must be more or equals then 10 second");
            int samples = totalTime / 10;
            int sampleLength = audio.Length / samples;
            if (!FftSharp.Transform.IsPowerOfTwo(sampleLength)) sampleLength = (int)Math.Pow(2, Math.Floor(Math.Log2(sampleLength)));
            samples = audio.Length / sampleLength;

            List<KeyValuePair<double[], double[]>> data = new();
            for(int i = 0; i < samples; i++)
            {
                double[] sampleData = new FftSharp.Windows.Hanning().Apply(new List<double>(audio).GetRange(i * sampleLength, sampleLength).ToArray());
                if (!FftSharp.Transform.IsPowerOfTwo(sampleData.Length)) continue;

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
                List<double> freqPowers = new();
                for(int j = 0; j < freqs.Length; j++)
                {
                    if (freqs[j] >= (freq - 0.5) && freqs[j] <= (freq + 0.5)) freqPowers.Add(newPower[j]);
                }

                //double squaredFreq = Math.Sqrt(freqPowers.ConvertAll(item => Math.Pow(item, 2d)).Sum() / (freqPowers.Count * (freqPowers.Count - 1)));
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
            if (firstHash.Length != secondHash.Length) throw new ArgumentException("Hashes must be have equal length");

            List<int> firstInt = new List<string>(ChunkSplit(firstHash, 2)).ConvertAll(i => Convert.ToInt32(i));
            List<int> secondInt = new List<string>(ChunkSplit(secondHash, 2)).ConvertAll(i => Convert.ToInt32(i));

            List<int> diffs = new();
            for (int i = 0; i < firstInt.Count; i++) diffs.Add(Math.Abs(firstInt[i] - secondInt[i]));

            double percent = diffs.Average();
            return percent;
        }
    }
}
