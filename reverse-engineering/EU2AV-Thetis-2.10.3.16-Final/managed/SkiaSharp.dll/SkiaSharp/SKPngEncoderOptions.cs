using System;

namespace SkiaSharp;

public readonly struct SKPngEncoderOptions : IEquatable<SKPngEncoderOptions>
{
	public static readonly SKPngEncoderOptions Default;

	private readonly SKPngEncoderFilterFlags fFilterFlags;

	private readonly int fZLibLevel;

	private unsafe readonly void* fComments;

	private readonly IntPtr fICCProfile;

	private unsafe readonly void* fICCProfileDescription;

	public SKPngEncoderFilterFlags FilterFlags => fFilterFlags;

	public int ZLibLevel => fZLibLevel;

	static SKPngEncoderOptions()
	{
		Default = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 6);
	}

	public unsafe SKPngEncoderOptions(SKPngEncoderFilterFlags filterFlags, int zLibLevel)
	{
		fICCProfile = default(IntPtr);
		fICCProfileDescription = default(void*);
		fFilterFlags = filterFlags;
		fZLibLevel = zLibLevel;
		fComments = null;
	}

	public unsafe bool Equals(SKPngEncoderOptions obj)
	{
		if (fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel && fComments == obj.fComments && fICCProfile == obj.fICCProfile)
		{
			return fICCProfileDescription == obj.fICCProfileDescription;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKPngEncoderOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKPngEncoderOptions left, SKPngEncoderOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKPngEncoderOptions left, SKPngEncoderOptions right)
	{
		return !left.Equals(right);
	}

	public unsafe override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fFilterFlags);
		hashCode.Add(fZLibLevel);
		hashCode.Add(fComments);
		hashCode.Add(fICCProfile);
		hashCode.Add(fICCProfileDescription);
		return hashCode.ToHashCode();
	}
}
