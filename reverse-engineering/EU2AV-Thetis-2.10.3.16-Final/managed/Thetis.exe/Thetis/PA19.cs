using System;
using System.Runtime.InteropServices;

namespace Thetis;

public class PA19
{
	public enum PaErrorCode
	{
		paNoError = 0,
		paNotInitialized = -10000,
		paUnanticipatedHostError = -9999,
		paInvalidChannelCount = -9998,
		paInvalidSampleRate = -9997,
		paInvalidDevice = -9996,
		paInvalidFlag = -9995,
		paSampleFormatNotSupported = -9994,
		paBadIODeviceCombination = -9993,
		paInsufficientMemory = -9992,
		paBufferTooBig = -9991,
		paBufferTooSmall = -9990,
		paNullCallback = -9989,
		paBadStreamPtr = -9988,
		paTimedOut = -9987,
		paInternalError = -9986,
		paDeviceUnavailable = -9985,
		paIncompatibleHostApiSpecificStreamInfo = -9984,
		paStreamIsStopped = -9983,
		paStreamIsNotStopped = -9982,
		paInputOverflowed = -9981,
		paOutputUnderflowed = -9980,
		paHostApiNotFound = -9979,
		paInvalidHostApi = -9978,
		paCanNotReadFromACallbackStream = -9977,
		paCanNotWriteToACallbackStream = -9976,
		paCanNotReadFromAnOutputOnlyStream = -9975,
		paCanNotWriteToAnInputOnlyStream = -9974,
		paIncompatibleStreamHostApi = -9973
	}

	public enum PaHostApiTypeId
	{
		paInDevelopment = 0,
		paDirectSound = 1,
		paMME = 2,
		paASIO = 3,
		paSoundManager = 4,
		paCoreAudio = 5,
		paOSS = 7,
		paALSA = 8,
		paAL = 9,
		paBeOS = 10,
		paWDMKS = 11,
		paJACK = 12,
		paWASAPI = 13,
		paAudioScienceHPI = 14
	}

	public enum PaStreamCallbackResult
	{
		paContinue,
		paComplete,
		paAbort
	}

	public struct PaHostApiInfo
	{
		public int structVersion;

		public int type;

		private readonly IntPtr _name;

		public int deviceCount;

		public int defaultInputDevice;

		public int defaultOutputDevice;

		public string name
		{
			get
			{
				if (!(_name == IntPtr.Zero))
				{
					return Marshal.PtrToStringAnsi(_name);
				}
				return null;
			}
		}
	}

	public struct PaHostErrorInfo
	{
		public PaHostApiTypeId hostApiType;

		public int errorCode;

		[MarshalAs(UnmanagedType.LPStr)]
		public string errorText;
	}

	public struct PaDeviceInfo
	{
		public int structVersion;

		private IntPtr _name;

		public int hostApi;

		public int maxInputChannels;

		public int maxOutputChannels;

		public double defaultLowInputLatency;

		public double defaultLowOutputLatency;

		public double defaultHighInputLatency;

		public double defaultHighOutputLatency;

		public double defaultSampleRate;

		public string name
		{
			get
			{
				if (!(_name == IntPtr.Zero))
				{
					return Marshal.PtrToStringAnsi(_name);
				}
				return null;
			}
		}
	}

	public struct PaStreamParameters
	{
		public int device;

		public int channelCount;

		public uint sampleFormat;

		public double suggestedLatency;

		public unsafe void* hostApiSpecificStreamInfo;
	}

	public struct PaStreamCallbackTimeInfo
	{
		public double inputBufferAdcTime;

		public double currentTime;

		public double outputBufferDacTime;
	}

	public struct PaStreamInfo
	{
		public int structVersion;

		public double inputLatency;

		public double outputLatency;

		public double sampleRate;
	}

	public unsafe delegate int PaStreamCallback(void* input, void* output, int frameCount, PaStreamCallbackTimeInfo* timeInfo, int statusFlags, void* userData);

	public unsafe delegate void PaStreamFinishedCallback(void* userData);

	public const int paNoDevice = -1;

	public const int paUseHostApiSpecificDeviceSpecification = -2;

	public const uint paFloat32 = 1u;

	public const uint paInt32 = 2u;

	public const uint paInt24 = 4u;

	public const uint paInt16 = 8u;

	public const uint paInt8 = 16u;

	public const uint paUInt8 = 32u;

	public const uint paCustomFormat = 65536u;

	public const uint paNonInterleaved = 2147483648u;

	public const uint paFormatIsSupported = 0u;

	public const uint paFramesPerBufferUnspecified = 0u;

	public const uint paNoFlag = 0u;

	public const uint paClipOff = 1u;

	public const uint paDitherOff = 2u;

	public const uint paNeverDropInput = 4u;

	public const uint paPrimeOutputBuffersUsingStreamCallback = 8u;

	public const uint paPlatformSpecificFlags = 4294901760u;

	public const uint paInputUnderflow = 1u;

	public const uint paInputOverflow = 2u;

	public const uint paOutputUnderflow = 4u;

	public const uint paOutputOverflow = 8u;

	public const uint paPrimingOutput = 16u;

	[DllImport("PA19.dll")]
	public static extern int PA_GetVersion();

	[DllImport("PA19.dll")]
	public static extern string PA_GetVersionText();

	[DllImport("PA19.dll", EntryPoint = "PA_GetErrorText")]
	public static extern IntPtr IntPtr_PA_GetErrorText(int error);

	public static string PA_GetErrorText(int error)
	{
		return Marshal.PtrToStringAnsi(IntPtr_PA_GetErrorText(error));
	}

	[DllImport("PA19.dll")]
	public static extern int PA_Initialize();

	[DllImport("PA19.dll")]
	public static extern int PA_Terminate();

	[DllImport("PA19.dll")]
	public static extern int PA_GetHostApiCount();

	[DllImport("PA19.dll")]
	public static extern int PA_GetDefaultHostApi();

	[DllImport("PA19.dll", EntryPoint = "PA_GetHostApiInfo")]
	public static extern IntPtr PA_GetHostApiInfoPtr(int hostId);

	public static PaHostApiInfo PA_GetHostApiInfo(int hostId)
	{
		return (PaHostApiInfo)Marshal.PtrToStructure(PA_GetHostApiInfoPtr(hostId), typeof(PaHostApiInfo));
	}

	[DllImport("PA19.dll")]
	public static extern int PA_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type);

	[DllImport("PA19.dll")]
	public static extern int PA_HostApiDeviceIndexToDeviceIndex(int hostAPI, int hostApiDeviceIndex);

	[DllImport("PA19.dll", EntryPoint = "PA_GetLastHostErrorInfo")]
	public static extern IntPtr PA_GetLastHostErrorInfoPtr();

	public static PaHostErrorInfo PA_GetLastHostErrorInfo()
	{
		return (PaHostErrorInfo)Marshal.PtrToStructure(PA_GetLastHostErrorInfoPtr(), typeof(PaHostErrorInfo));
	}

	[DllImport("PA19.dll")]
	public static extern int PA_GetDeviceCount();

	[DllImport("PA19.dll")]
	public static extern int PA_GetDefaultInputDevice();

	[DllImport("PA19.dll")]
	public static extern int PA_GetDefaultOutputDevice();

	[DllImport("PA19.dll", EntryPoint = "PA_GetDeviceInfo")]
	public static extern IntPtr PA_GetDeviceInfoPtr(int device);

	public static PaDeviceInfo PA_GetDeviceInfo(int device)
	{
		return (PaDeviceInfo)Marshal.PtrToStructure(PA_GetDeviceInfoPtr(device), typeof(PaDeviceInfo));
	}

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_IsFormatSupported(PaStreamParameters* inputParameters, PaStreamParameters* outputParameters, double sampleRate);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_OpenStream(out void* stream, PaStreamParameters* inputParameters, PaStreamParameters* outputParameters, double sampleRate, uint framesPerBuffer, uint streamFlags, PaStreamCallback streamCallback, int callback_id);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_OpenDefaultStream(out void* stream, int numInputChannels, int numOutputChannels, uint sampleFormat, double sampleRate, uint framesPerBuffer, PaStreamCallback streamCallback, int callback_id);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_CloseStream(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_SetStreamFinishedCallback(void* stream, PaStreamFinishedCallback streamFinishedCallback);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_StartStream(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_StopStream(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_AbortStream(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_IsStreamStopped(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_IsStreamActive(void* stream);

	[DllImport("PA19.dll", EntryPoint = "PA_GetStreamInfo")]
	public unsafe static extern IntPtr PA_GetStreamInfoPtr(void* stream);

	public unsafe static PaStreamInfo PA_GetStreamInfo(void* stream)
	{
		return (PaStreamInfo)Marshal.PtrToStructure(PA_GetStreamInfoPtr(stream), typeof(PaStreamInfo));
	}

	[DllImport("PA19.dll")]
	public unsafe static extern double PA_GetStreamTime(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern double PA_GetStreamCpuLoad(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_ReadStream(void* stream, void* buffer, uint frames);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_WriteStream(void* stream, void* buffer, uint frames);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_GetStreamReadAvailable(void* stream);

	[DllImport("PA19.dll")]
	public unsafe static extern int PA_GetStreamWriteAvailable(void* stream);

	[DllImport("PA19.dll")]
	public static extern int PA_GetSampleSize(uint format);

	[DllImport("PA19.dll")]
	public static extern void PA_Sleep(int msec);
}
