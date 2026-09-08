using System;

namespace SkiaSharp;

public readonly struct SKCubicResampler(float b, float c) : IEquatable<SKCubicResampler>
{
	public static readonly SKCubicResampler Mitchell = new SKCubicResampler(1f / 3f, 1f / 3f);

	public static readonly SKCubicResampler CatmullRom = new SKCubicResampler(0f, 0.5f);

	private readonly float fB = b;

	private readonly float fC = c;

	public float B => fB;

	public float C => fC;

	public bool Equals(SKCubicResampler obj)
	{
		if (fB == obj.fB)
		{
			return fC == obj.fC;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKCubicResampler obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKCubicResampler left, SKCubicResampler right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKCubicResampler left, SKCubicResampler right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fB);
		hashCode.Add(fC);
		return hashCode.ToHashCode();
	}
}
