using System;

namespace SDRSharp.Radio
{
    public unsafe sealed class Vfo
    {
        private const float TimeConst = 0.01f;
        public const int DefaultCwSideTone = 600;
        public const int DefaultSSBBandwidth = 2400;
        public const int DefaultWFMBandwidth = 180000;
        public const int MinSSBAudioFrequency = 100;
        public const int MinBCAudioFrequency = 20;
        public const int MaxBCAudioFrequency = 16000;
        public const int MaxNFMBandwidth = 15000;
        public const int MinNFMAudioFrequency = 300;

        private readonly double _minThreadedSampleRate = Utils.GetDoubleSetting("minThreadedSampleRate", 1000000);

        private readonly AutomaticGainControl _agc = new AutomaticGainControl();
        private readonly AmDetector _amDetector = new AmDetector();
        private readonly FmDetector _fmDetector = new FmDetector();
        private readonly LsbDetector _lsbDetector = new LsbDetector();
        private readonly UsbDetector _usbDetector = new UsbDetector();
        private readonly CwDetector _cwDetector = new CwDetector();
        private readonly DsbDetector _dsbDetector = new DsbDetector();
        private readonly StereoDecoder _stereoDecoder = new StereoDecoder();
        private readonly RdsDecoder _rdsDecoder = new RdsDecoder();
        private readonly FirFilter _audioFilter = new FirFilter();
        private DownConverter _downConverter;
        private DcRemover _dcRemover = new DcRemover(TimeConst);
        private IQFirFilter _iqFilter;
        private IQDecimator _baseBandDecimator;
        private DetectorType _detectorType;
        private DetectorType _actualDetectorType;
        private WindowType _windowType;
        private double _sampleRate;
        private int _bandwidth;
        private int _frequency;
        private int _filterOrder;
        private bool _needNewFilters;
        private int _decimationStageCount;
        private int _baseBandDecimationStageCount;
        private int _audioDecimationStageCount;
        private bool _needNewDecimators;
        private bool _decimationModeHasChanged;
        private int _cwToneShift;
        private bool _needConfigure;
        private bool _useAgc;
        private float _agcThreshold;
        private float _agcDecay;
        private float _agcSlope;
        private bool _agcUseHang;
        private int _squelchThreshold;
        private bool _fmStereo;
        private bool _filterAudio;
        private UnsafeBuffer _rawAudioBuffer;
        private float* _rawAudioPtr;

        public Vfo()
        {
            _bandwidth = DefaultSSBBandwidth;
            _filterOrder = FilterBuilder.DefaultFilterOrder;
            _needConfigure = true;
        }

        public DetectorType DetectorType
        {
            get
            {
                return _detectorType;
            }
            set
            {
                if (value != _detectorType)
                {
                    _decimationModeHasChanged = (_detectorType == DetectorType.WFM && value != DetectorType.WFM) ||
                                         (_detectorType != DetectorType.WFM && value == DetectorType.WFM);
                    _needNewDecimators = _decimationModeHasChanged;
                    _detectorType = value;
                    _needConfigure = true;
                }
            }
        }

        public int Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    _needConfigure = true;
                }
            }
        }

        public int FilterOrder
        {
            get
            {
                return _filterOrder;
            }
            set
            {
                if (_filterOrder != value)
                {
                    _filterOrder = value;
                    _needNewFilters = true;
                    _needConfigure = true;
                }
            }
        }

        public double SampleRate
        {
            get
            {
                return _sampleRate;
            }
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    _needNewDecimators = true;
                    _needConfigure = true;
                }
            }
        }

        public WindowType WindowType
        {
            get
            {
                return _windowType;
            }
            set
            {
                if (_windowType != value)
                {
                    _windowType = value;
                    _needNewFilters = true;
                    _needConfigure = true;
                }
            }
        }

        public int Bandwidth
        {
            get
            {
                return _bandwidth;
            }
            set
            {
                if (_bandwidth != value)
                {
                    _bandwidth = value;
                    _needNewFilters = true;
                    _needConfigure = true;
                }
            }
        }

        public bool UseAGC
        {
            get { return _useAgc; }
            set { _useAgc = value; }
        }

        public float AgcThreshold
        {
            get { return _agcThreshold; }
            set
            {
                if (_agcThreshold != value)
                {
                    _agcThreshold = value;
                    _needConfigure = true;
                }
            }
        }

        public float AgcDecay
        {
            get { return _agcDecay; }
            set
            {
                if (_agcDecay != value)
                {
                    _agcDecay = value;
                    _needConfigure = true;
                }
            }
        }

        public float AgcSlope
        {
            get { return _agcSlope; }
            set
            {
                if (_agcSlope != value)
                {
                    _agcSlope = value;
                    _needConfigure = true;
                }
            }
        }

        public bool AgcHang
        {
            get { return _agcUseHang; }
            set
            {
                if (_agcUseHang != value)
                {
                    _agcUseHang = value;
                    _needConfigure = true;
                }
            }
        }

        public int SquelchThreshold
        {
            get { return _squelchThreshold; }
            set
            {
                if (_squelchThreshold != value)
                {
                    _squelchThreshold = value;
                    _needConfigure = true;
                }
            }
        }

        public bool IsSquelchOpen
        {
            get
            {
                return (_actualDetectorType == DetectorType.NFM && _fmDetector.IsSquelchOpen) ||
                       (_actualDetectorType == DetectorType.AM && _amDetector.IsSquelchOpen);
            }
        }

        public int DecimationStageCount
        {
            get { return _decimationStageCount; }
            set
            {
                if (_decimationStageCount != value)
                {
                    _decimationStageCount = value;
                    _needNewDecimators = true;
                    _needConfigure = true;
                }
            }
        }

        public int CWToneShift
        {
            get { return _cwToneShift; }
            set
            {
                if (_cwToneShift != value)
                {
                    _cwToneShift = value;
                    _needNewFilters = true;
                    _needConfigure = true;
                }
            }
        }

        public bool FmStereo
        {
            get { return _fmStereo; }
            set
            {
                if (_fmStereo != value)
                {
                    _fmStereo = value;
                    _needConfigure = true;
                }
            }
        }

        public bool SignalIsStereo
        {
            get { return _actualDetectorType == DetectorType.WFM && _fmStereo && _stereoDecoder.IsPllLocked; }
        }

        public string RdsStationName
        {
            get { return _rdsDecoder.ProgramService; }
        }

        public string RdsStationText
        {
            get { return _rdsDecoder.RadioText; }
        }

        public ushort RdsPICode
        {
            get { return _rdsDecoder.PICode; }
        }

        public bool FilterAudio
        {
            get { return _filterAudio; }
            set { _filterAudio = value; }
        }

        public void RdsReset()
        {
            _rdsDecoder.Reset();
        }

        private void Configure()
        {
            _actualDetectorType = _detectorType;
            var multiThreaded = _sampleRate >= _minThreadedSampleRate;
            if (_downConverter == null || (multiThreaded && _downConverter.PhaseCount > 1))
            {
                _downConverter = new DownConverter(multiThreaded ? Environment.ProcessorCount : 1);
            }
            _downConverter.SampleRate = _sampleRate;
            _downConverter.Frequency = _frequency;
            if (_needNewDecimators || _baseBandDecimator == null)
            {
                _needNewDecimators = false;
                if (_actualDetectorType == DetectorType.WFM)
                {
                    var afSamplerate = _sampleRate / Math.Pow(2.0, _decimationStageCount);
                    _audioDecimationStageCount = 0;
                    while (afSamplerate * Math.Pow(2.0, _audioDecimationStageCount) < DefaultWFMBandwidth && _audioDecimationStageCount < _decimationStageCount)
                    {
                        _audioDecimationStageCount++;
                    }
                    _baseBandDecimationStageCount = _decimationStageCount - _audioDecimationStageCount;
                }
                else
                {
                    _baseBandDecimationStageCount = _decimationStageCount;
                    _audioDecimationStageCount = 0;
                }
                _baseBandDecimator = new IQDecimator(_baseBandDecimationStageCount, _sampleRate, false, Environment.ProcessorCount > 1);
                _needNewFilters = true;
            }
            if (_needNewFilters)
            {
                _needNewFilters = false;
                InitFilters();
            }
            var baseBandSampleRate = _sampleRate / Math.Pow(2.0, _baseBandDecimationStageCount);
            _usbDetector.SampleRate = baseBandSampleRate;
            _lsbDetector.SampleRate = baseBandSampleRate;
            _cwDetector.SampleRate = baseBandSampleRate;
            _fmDetector.SampleRate = baseBandSampleRate;
            _fmDetector.SquelchThreshold = _squelchThreshold;
            _amDetector.SquelchThreshold = _squelchThreshold;
            _stereoDecoder.Configure(_fmDetector.SampleRate, _audioDecimationStageCount);
            _rdsDecoder.SampleRate = _fmDetector.SampleRate;
            _stereoDecoder.ForceMono = !_fmStereo;
            switch (_actualDetectorType)
            {
                case DetectorType.USB:
                    _usbDetector.BfoFrequency = -_bandwidth / 2;
                    _downConverter.Frequency -= _usbDetector.BfoFrequency;
                    break;

                case DetectorType.LSB:
                    _lsbDetector.BfoFrequency = -_bandwidth / 2;
                    _downConverter.Frequency += _lsbDetector.BfoFrequency;
                    break;

                case DetectorType.CW:
                    _cwDetector.BfoFrequency = _cwToneShift;
                    break;

                case DetectorType.NFM:
                    _fmDetector.Mode = FmMode.Narrow;
                    break;

                case DetectorType.WFM:
                    _fmDetector.Mode = FmMode.Wide;
                    break;
            }

            _agc.SampleRate = _sampleRate / Math.Pow(2.0, _decimationStageCount);
            _agc.Decay = _agcDecay;
            _agc.Slope = _agcSlope;
            _agc.Threshold = _agcThreshold;
            _agc.UseHang = _agcUseHang;

            _decimationModeHasChanged = false;
        }

        private void InitFilters()
        {
            int cutoff1 = 0;
            int cutoff2 = 10000;
            var iqBW = _bandwidth / 2;
            int iqOrder = _actualDetectorType == DetectorType.WFM ? 60 : _filterOrder;
            
            var coeffs = FilterBuilder.MakeLowPassKernel(_sampleRate / Math.Pow(2.0, _baseBandDecimationStageCount), iqOrder, iqBW, _windowType);

            if (_iqFilter == null || _decimationModeHasChanged)
            {
                _iqFilter = new IQFirFilter(coeffs, _actualDetectorType == DetectorType.WFM, 1);
            }
            else
            {
                _iqFilter.SetCoefficients(coeffs);
            }

            switch (_actualDetectorType)
            {
                case DetectorType.AM:
                    cutoff1 = MinBCAudioFrequency;
                    cutoff2 = Math.Min(_bandwidth / 2, MaxBCAudioFrequency);
                    break;

                case DetectorType.CW:
                    cutoff1 = Math.Abs(_cwToneShift) - _bandwidth / 2;
                    cutoff2 = Math.Abs(_cwToneShift) + _bandwidth / 2;
                    break;

                case DetectorType.USB:
                case DetectorType.LSB:
                    cutoff1 = MinSSBAudioFrequency;
                    cutoff2 = _bandwidth;
                    break;

                case DetectorType.DSB:
                    cutoff1 = MinSSBAudioFrequency;
                    cutoff2 = _bandwidth / 2;
                    break;

                case DetectorType.NFM:
                    cutoff1 = MinNFMAudioFrequency;
                    cutoff2 = _bandwidth / 2;
                    break;
            }

            coeffs = FilterBuilder.MakeBandPassKernel(_sampleRate / Math.Pow(2.0, _baseBandDecimationStageCount + _audioDecimationStageCount), _filterOrder, cutoff1, cutoff2, _windowType);
            _audioFilter.SetCoefficients(coeffs);
        }

        public void ProcessBuffer(Complex* iqBuffer, float* audioBuffer, int length)
        {
            if (_needConfigure)
            {
                Configure();
                _needConfigure = false;
            }

            _downConverter.Process(iqBuffer, length);

            if (_baseBandDecimator.StageCount > 0)
            {
                _baseBandDecimator.Process(iqBuffer, length);
                length /= (int) Math.Pow(2.0, _baseBandDecimator.StageCount);
            }

            _iqFilter.Process(iqBuffer, length);

            if (_actualDetectorType == DetectorType.RAW)
            {
                Utils.Memcpy(audioBuffer, iqBuffer, length * sizeof(Complex));
                return;
            }

            if (_rawAudioBuffer == null || _rawAudioBuffer.Length != length)
            {
                _rawAudioBuffer = UnsafeBuffer.Create(length, sizeof (float));
                _rawAudioPtr = (float*) _rawAudioBuffer;
            }

            if (_actualDetectorType != DetectorType.WFM)
            {
                ScaleIQ(iqBuffer, length);
            }

            Demodulate(iqBuffer, _rawAudioPtr, length);

            if (_actualDetectorType != DetectorType.WFM)
            {
                if (_filterAudio)
                {
                    _audioFilter.Process(_rawAudioPtr, length);
                }

                if (_actualDetectorType != DetectorType.NFM && _useAgc)
                {
                    _agc.Process(_rawAudioPtr, length);
                }
            }

            if (_actualDetectorType == DetectorType.AM)
            {
                _dcRemover.Process(_rawAudioPtr, length);
            }

            if (_actualDetectorType == DetectorType.WFM)
            {
                _rdsDecoder.Process(_rawAudioPtr, length);
                _stereoDecoder.Process(_rawAudioPtr, audioBuffer, length);
            }
            else
            {
                MonoToStereo(_rawAudioPtr, audioBuffer, length);
            }
        }

        private static void ScaleIQ(Complex* buffer, int length)
        {
            for (var i = 0; i < length; i++)
            {
                buffer[i].Real *= 0.01f;
                buffer[i].Imag *= 0.01f;
            }
        }

        private static void MonoToStereo(float* input, float* output, int inputLength)
        {
            for (var i = 0; i < inputLength; i++)
            {
                *output++ = *input;
                *output++ = *input;
                input++;
            }
        }

        private void Demodulate(Complex* iq, float* audio, int length)
        {
            switch (_actualDetectorType)
            {
                case DetectorType.WFM:
                case DetectorType.NFM:
                    _fmDetector.Demodulate(iq, audio, length);
                    break;

                case DetectorType.AM:
                    _amDetector.Demodulate(iq, audio, length);
                    break;

                case DetectorType.DSB:
                    _dsbDetector.Demodulate(iq, audio, length);
                    break;

                case DetectorType.LSB:
                    _lsbDetector.Demodulate(iq, audio, length);
                    break;

                case DetectorType.USB:
                    _usbDetector.Demodulate(iq, audio, length);
                    break;

                case DetectorType.CW:
                    _cwDetector.Demodulate(iq, audio, length);
                    break;
            }
        }
    }
}