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

        public static (double[] audio, int sampleRate) ReadAudioFile(FileInfo file, double multiplier = 16__000)
        {
            using var afr = new AudioFileReader(file.FullName);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelsCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelsCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0) audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
        }

        public static (double[] power, double[] freqs) MakeFFT(double[] audio, int sampleRate)
        {
            if (!FftSharp.Transform.IsPowerOfTwo(audio.Length))
            {
                int correctLength = (int)Math.Pow(2, Math.Floor(Math.Log2(audio.Length)));
                audio = new List<double>(audio).GetRange(0, correctLength).ToArray();
            }

            double[] newAudio = new FftSharp.Windows.Hanning().Apply(audio);
            double[] power = FftSharp.Transform.FFTpower(newAudio);
            double[] freqs = FftSharp.Transform.FFTfreq(sampleRate, power);

            return (power, freqs);
        }

        public static (double[] power, double[] freqs) PrepareAudioData(double[] power, double[] freqs)
        {
            double minPower = power.Min();
            List<double> newPower = new List<double>(power).ConvertAll(item => item - minPower);

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
                double squaredFreq = freqPowers.Average();
                powerFreqs[freq] = squaredFreq;
            }

            minPower = powerFreqs.Values.Min();
            powerFreqs = new(powerFreqs.ToDictionary(item => item.Key, item => item.Value - minPower));
            return (powerFreqs.Values.ToArray(), powerFreqs.Keys.ToArray());
        }

        public static int[] GetAudioHash(double[] audio, int sampleRate)
        {
            (double[] powers, double[] freqs) = MakeFFT(audio, sampleRate);
            (double[] newPowers, double[] newFreqs) = PrepareAudioData(powers, freqs);
            return GetAudioHash(newPowers, newFreqs);
        }

        public static int[] GetAudioHash(double[] powers, double[] freqs)
        {
            List<int> hash = new();
            SortedDictionary<double, double> newPowerData = new();
            for (int i = 1; i < freqs[^1] + 1; i++) newPowerData[i] = 0;

            for(int i = 0; i < freqs.Length; i++) newPowerData[freqs[i]] = powers[i];

            return hash.ToArray();
        }
    }
}
