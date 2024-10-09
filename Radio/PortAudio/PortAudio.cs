 /*
  * PortAudioSharp - PortAudio bindings for .NET
  * Copyright 2006-2011 Riccardo Gerosa and individual contributors as indicated
  * by the @authors tag. See the copyright.txt in the distribution for a
  * full listing of individual contributors.
  *
  * Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
  * and associated documentation files (the "Software"), to deal in the Software without restriction, 
  * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
  * subject to the following conditions:
  *
  * The above copyright notice and this permission notice shall be included in all copies or substantial 
  * portions of the Software.
  *
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
  * NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
  * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
  * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
  */

using System;
using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	///	<summary>
	///		PortAudio v.19 bindings for .NET
	///	</summary>
	static class PortAudioAPI
    {
        #region **** PORTAUDIO CONSTANTS ****

        public const int PaFormatIsSupported = 0;
        public const int PaFramesPerBufferUnspecified = 0;
		
        private const string PortAudioLibrary = "portaudio";

        #endregion

        #region **** PORTAUDIO FUNCTIONS ****

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetVersion();
	 	
	 	[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetVersionText")]
	 	private static extern IntPtr IntPtr_Pa_GetVersionText();
	 	
	 	public static string Pa_GetVersionText() {
	 		IntPtr strptr = IntPtr_Pa_GetVersionText();
	 		return Marshal.PtrToStringAnsi(strptr);
	 	}
	 	
	 	[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetErrorText")]
	 	public static extern IntPtr IntPtr_Pa_GetErrorText(PaError errorCode);
	 	
	 	public static string Pa_GetErrorText(PaError errorCode) {
	 		IntPtr strptr = IntPtr_Pa_GetErrorText(errorCode);
	 		return Marshal.PtrToStringAnsi(strptr);
	 	}
	 	
	 	[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_Initialize();
	 	
	 	[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_Terminate();
	 	
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetHostApiCount();

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetDefaultHostApi();

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetHostApiInfo")]
	 	public static extern IntPtr IntPtr_Pa_GetHostApiInfo(int hostApi);
	 	
	 	public static PaHostApiInfo Pa_GetHostApiInfo(int hostApi) {
	 		IntPtr structptr = IntPtr_Pa_GetHostApiInfo(hostApi);
	 		return (PaHostApiInfo) Marshal.PtrToStructure(structptr, typeof(PaHostApiInfo));
	 	}
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type);

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_HostApiDeviceIndexToDeviceIndex(int hostApi, int hostApiDeviceIndex);

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetLastHostErrorInfo")]
	 	public static extern IntPtr IntPtr_Pa_GetLastHostErrorInfo();
	 	
	 	public static PaHostErrorInfo Pa_GetLastHostErrorInfo() {
	 		IntPtr structptr = IntPtr_Pa_GetLastHostErrorInfo();
	 		return (PaHostErrorInfo) Marshal.PtrToStructure(structptr, typeof(PaHostErrorInfo));
	 	}

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetDeviceCount();
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetDefaultInputDevice();

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetDefaultOutputDevice();
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetDeviceInfo")]
	 	public static extern IntPtr IntPtr_Pa_GetDeviceInfo(int device);
	 	
	 	public static PaDeviceInfo Pa_GetDeviceInfo(int device) {
	 		IntPtr structptr = IntPtr_Pa_GetDeviceInfo(device);
	 		return (PaDeviceInfo) Marshal.PtrToStructure(structptr, typeof(PaDeviceInfo));
        }

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_IsFormatSupported(
            ref PaStreamParameters inputParameters,
            ref PaStreamParameters outputParameters,
            double sampleRate);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_IsFormatSupported(
            IntPtr inputParameters,
            ref PaStreamParameters outputParameters,
            double sampleRate);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_IsFormatSupported(
            ref PaStreamParameters inputParameters,
            IntPtr outputParameters,
            double sampleRate);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_OpenStream(
            out IntPtr stream,
            ref PaStreamParameters inputParameters,
            ref PaStreamParameters outputParameters,
            double sampleRate,
            uint framesPerBuffer,
            PaStreamFlags streamFlags,
            PaStreamCallbackDelegate streamCallback,
            IntPtr userData);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_OpenStream(
            out IntPtr stream,
            IntPtr inputParameters,
            ref PaStreamParameters outputParameters,
            double sampleRate,
            uint framesPerBuffer,
            PaStreamFlags streamFlags,
            PaStreamCallbackDelegate streamCallback,
            IntPtr userData);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_OpenStream(
            out IntPtr stream,
            ref PaStreamParameters inputParameters,
            IntPtr outputParameters,
            double sampleRate,
            uint framesPerBuffer,
            PaStreamFlags streamFlags,
            PaStreamCallbackDelegate streamCallback,
            IntPtr userData);

		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_OpenDefaultStream(
	 		out IntPtr stream,
	 		int numInputChannels, 
	 		int numOutputChannels, 
	 		uint sampleFormat,
	 		double sampleRate, 
	 		uint framesPerBuffer,
	 		PaStreamCallbackDelegate streamCallback,
	 		IntPtr userData);
	 		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_CloseStream(IntPtr stream);
	 	
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_SetStreamFinishedCallback(
	 		ref IntPtr stream,
	 		[MarshalAs(UnmanagedType.FunctionPtr)]PaStreamFinishedCallbackDelegate streamFinishedCallback);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_StartStream(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_StopStream(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_AbortStream(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_IsStreamStopped(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_IsStreamActive(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl,EntryPoint="Pa_GetStreamInfo")]
	 	public static extern IntPtr IntPtr_Pa_GetStreamInfo(IntPtr stream);
	 	
	 	public static PaStreamInfo Pa_GetStreamInfo(IntPtr stream) {
	 		IntPtr structptr = IntPtr_Pa_GetStreamInfo(stream);
	 		return (PaStreamInfo) Marshal.PtrToStructure(structptr,typeof(PaStreamInfo));
	 	}
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern double Pa_GetStreamTime(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern double Pa_GetStreamCpuLoad(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]float[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]byte[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]sbyte[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]ushort[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]short[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]uint[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_ReadStream(
	 		IntPtr stream,
	 		[Out]int[] buffer,
            uint frames);

        [DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_ReadStream(
            IntPtr stream,
            IntPtr buffer,
            uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]float[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]byte[] buffer,
			uint frames);
				
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]sbyte[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]ushort[] buffer,
			uint frames);
			
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]short[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]uint[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_WriteStream(
	 		IntPtr stream,
	 		[In]int[] buffer,
			uint frames);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetStreamReadAvailable(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern int Pa_GetStreamWriteAvailable(IntPtr stream);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern PaError Pa_GetSampleSize(PaSampleFormat format);
		
		[DllImport(PortAudioLibrary, CallingConvention = CallingConvention.Cdecl)]
	 	public static extern void Pa_Sleep(int msec);
	 	
	 	#endregion

        static PortAudioAPI()
        {
            Pa_Initialize();
        }
	}
    
    #region **** PORTAUDIO CALLBACKS ****
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate PaStreamCallbackResult PaStreamCallbackDelegate(
 		float* input,
 		float* output,
 		uint frameCount, 
 		ref PaStreamCallbackTimeInfo timeInfo,
 		PaStreamCallbackFlags statusFlags, 
 		IntPtr userData);
 	
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
 	delegate void PaStreamFinishedCallbackDelegate(IntPtr userData);
 	
 	#endregion
 	
	#region **** PORTAUDIO DATA STRUCTURES ****
	
	[StructLayout (LayoutKind.Sequential)]
	struct PaDeviceInfo {
		
		public int structVersion;
		[MarshalAs (UnmanagedType.LPStr)]
		public string name;
		public int hostApi;
		public int maxInputChannels;
		public int maxOutputChannels;
		public double defaultLowInputLatency;
		public double defaultLowOutputLatency;
		public double defaultHighInputLatency;
		public double defaultHighOutputLatency;
		public double defaultSampleRate;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "name: " + name + "\n"
				+ "hostApi: " + hostApi + "\n"
				+ "maxInputChannels: " + maxInputChannels + "\n"
				+ "maxOutputChannels: " + maxOutputChannels + "\n"
				+ "defaultLowInputLatency: " + defaultLowInputLatency + "\n"
				+ "defaultLowOutputLatency: " + defaultLowOutputLatency + "\n"
				+ "defaultHighInputLatency: " + defaultHighInputLatency + "\n"
				+ "defaultHighOutputLatency: " + defaultHighOutputLatency + "\n"
				+ "defaultSampleRate: " + defaultSampleRate;
		}
	}
	
	[StructLayout (LayoutKind.Sequential)]
	struct PaHostApiInfo {
		
		public int structVersion;
		public PaHostApiTypeId type;
		[MarshalAs (UnmanagedType.LPStr)]
		public string name;
		public int deviceCount;
		public int defaultInputDevice;
		public int defaultOutputDevice;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "structVersion: " + structVersion + "\n"
				+ "type: " + type + "\n"
				+ "name: " + name + "\n"
				+ "deviceCount: " + deviceCount + "\n"
				+ "defaultInputDevice: " + defaultInputDevice + "\n"
				+ "defaultOutputDevice: " + defaultOutputDevice;
		}
	}
	
	[StructLayout (LayoutKind.Sequential)]
	struct PaHostErrorInfo {
		
		public PaHostApiTypeId 	hostApiType;
		public int errorCode;
		[MarshalAs (UnmanagedType.LPStr)]
		public string errorText;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "hostApiType: " + hostApiType + "\n"
				+ "errorCode: " + errorCode + "\n"
				+ "errorText: " + errorText;
		}
	}
	
	[StructLayout (LayoutKind.Sequential)]
	struct PaStreamCallbackTimeInfo {
		
		public double inputBufferAdcTime;
 		public double currentTime;
		public double outputBufferDacTime;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "currentTime: " + currentTime + "\n"
				+ "inputBufferAdcTime: " + inputBufferAdcTime + "\n"
				+ "outputBufferDacTime: " + outputBufferDacTime;
		}
 	}
 	
 	[StructLayout (LayoutKind.Sequential)]
	struct PaStreamInfo {
 		
		public int structVersion;
		public double inputLatency;
		public double outputLatency;
		public double sampleRate;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "structVersion: " + structVersion + "\n"
				+ "inputLatency: " + inputLatency + "\n"
				+ "outputLatency: " + outputLatency + "\n"
				+ "sampleRate: " + sampleRate;
		}
	}
 	
	[StructLayout (LayoutKind.Sequential)]
	struct PaStreamParameters {
		
		public int device;
		public int channelCount;
		public PaSampleFormat sampleFormat;
		public double suggestedLatency;
		public IntPtr hostApiSpecificStreamInfo;
		
		public override string ToString() {
			return "[" + GetType().Name + "]" + "\n"
				+ "device: " + device + "\n"
				+ "channelCount: " + channelCount + "\n"
				+ "sampleFormat: " + sampleFormat + "\n"
				+ "suggestedLatency: " + suggestedLatency;
		}
	}
 		
	#endregion
	
	#region **** PORTAUDIO DEFINES ****
	
	enum PaDeviceIndex
	{
		PaNoDevice = -1,
		PaUseHostApiSpecificDeviceSpecification = -2
	}

	enum PaSampleFormat: uint
	{
	   PaFloat32 = 0x00000001,
	   PaInt32 = 0x00000002,
	   PaInt24 = 0x00000004,
	   PaInt16 = 0x00000008,
	   PaInt8 = 0x00000010,
	   PaUInt8 = 0x00000020,
	   PaCustomFormat = 0x00010000,
	   PaNonInterleaved = 0x80000000,
	}
	
	enum PaStreamFlags: uint
	{
		PaNoFlag = 0,
		PaClipOff = 0x00000001,
		PaDitherOff = 0x00000002,
		PaNeverDropInput = 0x00000004,
		PaPrimeOutputBuffersUsingStreamCallback = 0x00000008,
		PaPlatformSpecificFlags = 0xFFFF0000
	}
	
	enum PaStreamCallbackFlags: uint
	{
		PaInputUnderflow = 0x00000001,
		PaInputOverflow = 0x00000002,
		PaOutputUnderflow = 0x00000004,
		PaOutputOverflow = 0x00000008,
		PaPrimingOutput = 0x00000010
	}
	
	#endregion
	
	#region **** PORTAUDIO ENUMERATIONS ****
	
	enum PaError {
		paNoError = 0,
		paNotInitialized = -10000,
		paUnanticipatedHostError,
	    paInvalidChannelCount,
	    paInvalidSampleRate,
	    paInvalidDevice,
	    paInvalidFlag,
	    paSampleFormatNotSupported,
	    paBadIODeviceCombination,
	    paInsufficientMemory,
	    paBufferTooBig,
	    paBufferTooSmall,
	    paNullCallback,
	    paBadStreamPtr,
	    paTimedOut,
	    paInternalError,
	    paDeviceUnavailable,
	    paIncompatibleHostApiSpecificStreamInfo,
	    paStreamIsStopped,
	    paStreamIsNotStopped,
	    paInputOverflowed,
	    paOutputUnderflowed,
	    paHostApiNotFound,
	    paInvalidHostApi,
	    paCanNotReadFromACallbackStream,
	    paCanNotWriteToACallbackStream,
	    paCanNotReadFromAnOutputOnlyStream,
	    paCanNotWriteToAnInputOnlyStream,
	    paIncompatibleStreamHostApi,
	    paBadBufferPtr
	}
	
	enum PaHostApiTypeId : uint {
		paInDevelopment=0,
	    paDirectSound=1,
	    paMME=2,
	    paASIO=3,
	    paSoundManager=4,
	    paCoreAudio=5,
	    paOSS=7,
	    paALSA=8,
	    paAL=9,
	    paBeOS=10,
	    paWDMKS=11,
	    paJACK=12,
	    paWASAPI=13,
	    paAudioScienceHPI=14
	}
	
	enum PaStreamCallbackResult : uint { 
		PaContinue = 0, 
		PaComplete = 1, 
		PaAbort = 2 
	}
	
	#endregion
}
