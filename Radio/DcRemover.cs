﻿using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
#if !__MonoCS__
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
#endif
    public unsafe struct DcRemover
    {
        private float _average;
        private float _ratio;

        public DcRemover(float ratio)
        {
            _ratio = ratio;
            _average = 0.0f;
        }

        public void Init(float ratio)
        {
            _ratio = ratio;
            _average = 0.0f;
        }

        public float Offset
        {
            get { return _average; }
        }

        public void Process(float* buffer, int length)
        {
            for (var i = 0; i < length; i++)
            {
                _average += _ratio * (buffer[i] - _average);
                buffer[i] -= _average;
            }
        }

        public void ProcessInterleaved(float* buffer, int length)
        {
            length *= 2;

            for (var i = 0; i < length; i += 2)
            {
                _average += _ratio * (buffer[i] - _average);
                buffer[i] -= _average;
            }
        }

        public void Reset()
        {
            _average = 0.0f;
        }
    }
}
