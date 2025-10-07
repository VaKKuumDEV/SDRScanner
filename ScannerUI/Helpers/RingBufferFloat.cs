using System;

namespace ScannerUI.Helpers
{
    public class RingBufferFloat
    {
        private readonly float[] buf;
        private int pos;
        public int Count => buf.Length;
        public RingBufferFloat(int size) { buf = new float[size]; pos = 0; for (int i = 0; i < size; i++) buf[i] = 0f; }
        public void Push(float v) { buf[pos++] = v; if (pos >= buf.Length) pos = 0; }
        public float Mean()
        {
            double s = 0;
            foreach (var x in buf) s += x;
            return (float)(s / buf.Length);
        }
        public float StdDev(float mean)
        {
            double s = 0;
            foreach (var x in buf) { double d = x - mean; s += d * d; }
            return (float)Math.Sqrt(s / buf.Length);
        }
    }
}
