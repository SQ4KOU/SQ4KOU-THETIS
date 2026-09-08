using System;
using System.Runtime.InteropServices;
using NAudio.Dmo;
using NAudio.MediaFoundation;

namespace NAudio.Wave;

public class MediaFoundationResampler : MediaFoundationTransform
{
	private int resamplerQuality;

	public int ResamplerQuality
	{
		get
		{
			return resamplerQuality;
		}
		set
		{
			if (value < 1 || value > 60)
			{
				throw new ArgumentOutOfRangeException("Resampler Quality must be between 1 and 60");
			}
			resamplerQuality = value;
		}
	}

	private static bool IsPcmOrIeeeFloat(WaveFormat waveFormat)
	{
		WaveFormatExtensible waveFormatExtensible = waveFormat as WaveFormatExtensible;
		if (waveFormat.Encoding != WaveFormatEncoding.Pcm && waveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
		{
			if (waveFormatExtensible != null)
			{
				if (!(waveFormatExtensible.SubFormat == AudioSubtypes.MFAudioFormat_PCM))
				{
					return waveFormatExtensible.SubFormat == AudioSubtypes.MFAudioFormat_Float;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public MediaFoundationResampler(IWaveProvider sourceProvider, WaveFormat outputFormat)
		: base(sourceProvider, outputFormat)
	{
		if (!IsPcmOrIeeeFloat(sourceProvider.WaveFormat))
		{
			throw new ArgumentException("Input must be PCM or IEEE float", "sourceProvider");
		}
		if (!IsPcmOrIeeeFloat(outputFormat))
		{
			throw new ArgumentException("Output must be PCM or IEEE float", "outputFormat");
		}
		MediaFoundationApi.Startup();
		ResamplerQuality = 60;
		object comObject = CreateResamplerComObject();
		FreeComObject(comObject);
	}

	private void FreeComObject(object comObject)
	{
		Marshal.ReleaseComObject(comObject);
	}

	private object CreateResamplerComObject()
	{
		return new ResamplerMediaComObject();
	}

	public MediaFoundationResampler(IWaveProvider sourceProvider, int outputSampleRate)
		: this(sourceProvider, CreateOutputFormat(sourceProvider.WaveFormat, outputSampleRate))
	{
	}

	protected override IMFTransform CreateTransform()
	{
		object obj = CreateResamplerComObject();
		IMFTransform obj2 = (IMFTransform)obj;
		IMFMediaType iMFMediaType = MediaFoundationApi.CreateMediaTypeFromWaveFormat(sourceProvider.WaveFormat);
		obj2.SetInputType(0, iMFMediaType, _MFT_SET_TYPE_FLAGS.None);
		Marshal.ReleaseComObject(iMFMediaType);
		IMFMediaType iMFMediaType2 = MediaFoundationApi.CreateMediaTypeFromWaveFormat(outputWaveFormat);
		obj2.SetOutputType(0, iMFMediaType2, _MFT_SET_TYPE_FLAGS.None);
		Marshal.ReleaseComObject(iMFMediaType2);
		((IWMResamplerProps)obj).SetHalfFilterLength(ResamplerQuality);
		return obj2;
	}

	private static WaveFormat CreateOutputFormat(WaveFormat inputFormat, int outputSampleRate)
	{
		if (inputFormat.Encoding == WaveFormatEncoding.Pcm)
		{
			return new WaveFormat(outputSampleRate, inputFormat.BitsPerSample, inputFormat.Channels);
		}
		if (inputFormat.Encoding == WaveFormatEncoding.IeeeFloat)
		{
			return WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, inputFormat.Channels);
		}
		throw new ArgumentException("Can only resample PCM or IEEE float");
	}
}
