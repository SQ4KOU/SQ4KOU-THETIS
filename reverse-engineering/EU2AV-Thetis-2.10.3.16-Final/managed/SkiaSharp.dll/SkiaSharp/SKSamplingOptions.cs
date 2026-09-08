using System;

namespace SkiaSharp;

public readonly struct SKSamplingOptions : IEquatable<SKSamplingOptions>
{
	public static readonly SKSamplingOptions Default;

	private readonly int fMaxAniso;

	private readonly byte fUseCubic;

	private readonly SKCubicResampler fCubic;

	private readonly SKFilterMode fFilter;

	private readonly SKMipmapMode fMipmap;

	public bool IsAniso => MaxAniso != 0;

	public int MaxAniso => fMaxAniso;

	public bool UseCubic => fUseCubic > 0;

	public SKCubicResampler Cubic => fCubic;

	public SKFilterMode Filter => fFilter;

	public SKMipmapMode Mipmap => fMipmap;

	public SKSamplingOptions(SKFilterMode filter, SKMipmapMode mipmap)
	{
		fUseCubic = 0;
		fCubic = default(SKCubicResampler);
		fMaxAniso = 0;
		fFilter = filter;
		fMipmap = mipmap;
	}

	public SKSamplingOptions(SKFilterMode filter)
	{
		fUseCubic = 0;
		fCubic = default(SKCubicResampler);
		fMaxAniso = 0;
		fFilter = filter;
		fMipmap = SKMipmapMode.None;
	}

	public SKSamplingOptions(SKCubicResampler resampler)
	{
		fMaxAniso = 0;
		fFilter = SKFilterMode.Nearest;
		fMipmap = SKMipmapMode.None;
		fUseCubic = 1;
		fCubic = resampler;
	}

	public SKSamplingOptions(int maxAniso)
	{
		fUseCubic = 0;
		fCubic = default(SKCubicResampler);
		fFilter = SKFilterMode.Nearest;
		fMipmap = SKMipmapMode.None;
		fMaxAniso = Math.Max(1, maxAniso);
	}

	public bool Equals(SKSamplingOptions obj)
	{
		if (fMaxAniso == obj.fMaxAniso && fUseCubic == obj.fUseCubic && fCubic == obj.fCubic && fFilter == obj.fFilter)
		{
			return fMipmap == obj.fMipmap;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKSamplingOptions obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKSamplingOptions left, SKSamplingOptions right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKSamplingOptions left, SKSamplingOptions right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fMaxAniso);
		hashCode.Add(fUseCubic);
		hashCode.Add(fCubic);
		hashCode.Add(fFilter);
		hashCode.Add(fMipmap);
		return hashCode.ToHashCode();
	}
}
