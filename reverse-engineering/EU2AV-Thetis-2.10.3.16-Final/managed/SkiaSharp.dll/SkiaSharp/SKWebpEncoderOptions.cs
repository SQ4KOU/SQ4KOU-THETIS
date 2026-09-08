using System;

namespace SkiaSharp;

public readonly struct SKWebpEncoderOptions : IEquatable<SKWebpEncoderOptions>
{
	public static readonly SKWebpEncoderOptions Default;

	private readonly SKWebpEncoderCompression fCompression;

	private readonly float fQuality;

	private readonly IntPtr fICCProfile;

	private unsafe readonly void* fICCProfileDescription;

	public SKWebpEncoderCompression Compression => fCompression;

	public float Quality => fQuality;

	static SKWebpEncoderOptions()
	{
		Default = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 100f);
	}

	public unsafe SKWebpEncoderOptions(SKWebpEncoderCompression compression, float quality)
	{
		fICCProfile = default(IntPtr);
		fICCProfileDescription = default(void*);
		fCompression = compression;
		fQuality = quality;
	}

	public unsafe bool Equals(SKWebpEncoderOptions obj)
	{
		if (fCompression == obj.fCompression && fQuality == obj.fQuality && fICCProfile == obj.fICCProfile)
		{
			return fICCProfileDescription == obj.fICCProfileDescription;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKWebpEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKWebpEncoderOptions left, SKWebpEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKWebpEncoderOptions left, SKWebpEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public unsafe override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fCompression);
		hashCode.Add(fQuality);
		hashCode.Add(fICCProfile);
		hashCode.Add(fICCProfileDescription);
		return hashCode.ToHashCode();
	}
}
