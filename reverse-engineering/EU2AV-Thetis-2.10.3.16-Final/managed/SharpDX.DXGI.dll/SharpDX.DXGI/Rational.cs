using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Rational(int numerator, int denominator) : IEquatable<Rational>
{
	public static readonly Rational Empty;

	public int Numerator = numerator;

	public int Denominator = denominator;

	public bool Equals(Rational other)
	{
		if (Numerator == other.Numerator)
		{
			return Denominator == other.Denominator;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Rational)
		{
			return Equals((Rational)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Numerator * 397) ^ Denominator;
	}

	public static bool operator ==(Rational left, Rational right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Rational left, Rational right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"{Numerator}/{Denominator}";
	}
}
