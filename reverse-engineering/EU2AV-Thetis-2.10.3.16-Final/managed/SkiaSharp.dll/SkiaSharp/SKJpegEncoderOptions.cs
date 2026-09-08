using System;

namespace SkiaSharp;

public readonly struct SKJpegEncoderOptions : IEquatable<SKJpegEncoderOptions>
{
	public static readonly SKJpegEncoderOptions Default;

	private readonly int fQuality;

	private readonly SKJpegEncoderDownsample fDownsample;

	private readonly SKJpegEncoderAlphaOption fAlphaOption;

	private readonly IntPtr xmpMetadata;

	private readonly IntPtr fICCProfile;

	private unsafe readonly void* fICCProfileDescription;

	public SKJpegEncoderAlphaOption AlphaOption => fAlphaOption;

	public SKJpegEncoderDownsample Downsample => fDownsample;

	public int Quality => fQuality;

	static SKJpegEncoderOptions()
	{
		Default = new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
	}

	public unsafe SKJpegEncoderOptions(int quality)
	{
		fICCProfile = default(IntPtr);
		fICCProfileDescription = default(void*);
		xmpMetadata = default(IntPtr);
		fQuality = quality;
		fDownsample = SKJpegEncoderDownsample.Downsample420;
		fAlphaOption = SKJpegEncoderAlphaOption.Ignore;
	}

	public unsafe SKJpegEncoderOptions(int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
	{
		fICCProfile = default(IntPtr);
		fICCProfileDescription = default(void*);
		xmpMetadata = default(IntPtr);
		fQuality = quality;
		fDownsample = downsample;
		fAlphaOption = alphaOption;
	}

	public unsafe bool Equals(SKJpegEncoderOptions obj)
	{
		if (fQuality == obj.fQuality && fDownsample == obj.fDownsample && fAlphaOption == obj.fAlphaOption && xmpMetadata == obj.xmpMetadata && fICCProfile == obj.fICCProfile)
		{
			return fICCProfileDescription == obj.fICCProfileDescription;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKJpegEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKJpegEncoderOptions left, SKJpegEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKJpegEncoderOptions left, SKJpegEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public unsafe override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fQuality);
		hashCode.Add(fDownsample);
		hashCode.Add(fAlphaOption);
		hashCode.Add(xmpMetadata);
		hashCode.Add(fICCProfile);
		hashCode.Add(fICCProfileDescription);
		return hashCode.ToHashCode();
	}
}
