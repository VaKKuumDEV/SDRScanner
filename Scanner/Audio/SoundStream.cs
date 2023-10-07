using System.Collections.Concurrent;

namespace Scanner.Audio
{
    public class SoundStream : Stream
    {
        private readonly long length;
        private long position;
        private readonly ConcurrentQueue<byte> sampleQueue;
        private readonly AutoResetEvent dataAvailableSignaler = new(false);
        private readonly int preloadSize = 2000;

        public SoundStream(int sampleRate = 8000)
        {
            length = int.MaxValue / 2 - 36;
            position = 0;

            //add wav header with too big length (near 70 hours for sample rate 8000)
            sampleQueue = new ConcurrentQueue<byte>(BuildWavHeader((int)length, sampleRate));
        }

        /// <summary>
        /// Write audio samples into stream
        /// </summary>
        public void Write(IEnumerable<short> samples)
        {
            //write samples to sample queue
            foreach (var sample in samples)
            {
                sampleQueue.Enqueue((byte)(sample & 0xFF));
                sampleQueue.Enqueue((byte)(sample >> 8));
            }

            //send signal to Read method
            if (sampleQueue.Count >= preloadSize)
                dataAvailableSignaler.Set();
        }

        /// <summary>
        /// Count of unread bytes in buffer
        /// </summary>
        public int Buffered
        {
            get { return sampleQueue.Count; }
        }

        /// <summary>
        /// Read
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position >= length)
                return 0;

            //wait while data will be available
            if (sampleQueue.Count < preloadSize)
                dataAvailableSignaler.WaitOne();

            var res = 0;

            //copy data from incoming queue to output buffer
            while (count > 0 && !sampleQueue.IsEmpty)
            {
                if (!sampleQueue.TryDequeue(out byte b)) return 0;
                buffer[offset + res] = b;
                count--;
                res++;
                position++;
            }

            return res;
        }

        #region WAV header

        public static byte[] BuildWavHeader(int samplesCount, int sampleRate = 8000)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            short frameSize = (short)(16 / 8);
            writer.Write(0x46464952);
            writer.Write(36 + samplesCount * frameSize);
            writer.Write(0x45564157);
            writer.Write(0x20746D66);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * frameSize);
            writer.Write(frameSize);
            writer.Write((short)16);
            writer.Write(0x61746164);
            writer.Write(samplesCount * frameSize);
            return stream.ToArray();
        }

        #endregion

        #region Stream impl

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get { return position; }
            set {; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
