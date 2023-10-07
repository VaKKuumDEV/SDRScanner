using NAudio.Wave;

namespace Scanner.Audio
{
    public class StreamPlayer : IDisposable
    {
        private readonly SoundStream stream;
        private readonly WaveOutEvent waveOut;
        private WaveFileReader reader;

        public StreamPlayer(int sampleRate = 8000)
        {
            stream = new SoundStream(sampleRate);
            waveOut = new WaveOutEvent();
        }

        /// <summary>
        /// Write audio samples into stream
        /// </summary>
        public void Write(params short[] samples)
        {
            stream.Write(samples);
        }

        /// <summary>
        /// Write audio samples into stream
        /// </summary>
        public void Write(IEnumerable<short> samples)
        {
            stream.Write(samples);
        }

        /// <summary>
        /// Plays sound
        /// </summary>
        public void PlayAsync()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                reader = new WaveFileReader(stream);
                waveOut.Init(reader);
                waveOut.Play();
            });
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public void Stop()
        {
            waveOut.Stop();
        }

        /// <summary>
        /// Volume
        /// </summary>
        public float Volume
        {
            get { return waveOut.Volume; }
            set { waveOut.Volume = value; }
        }

        /// <summary>
        /// Count of unread bytes in buffer
        /// </summary>
        public int Buffered
        {
            get { return stream.Buffered; }
        }

        public void Dispose()
        {
            waveOut.Dispose();
            reader.Dispose();
            stream.Dispose();
        }
    }
}
