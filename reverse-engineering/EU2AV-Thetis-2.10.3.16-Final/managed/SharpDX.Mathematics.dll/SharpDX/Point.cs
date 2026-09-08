using System;
using System.Runtime.CompilerServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

public struct Point(int x, int y) : IEquatable<Point>
{
	public static readonly Point Zero = new Point(0, 0);

	public int X = x;

	public int Y = y;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ref Point other)
	{
		if (other.X == X)
		{
			return other.Y == Y;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Point other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Point other))
		{
			return false;
		}
		return Equals(ref other);
	}

	public override int GetHashCode()
	{
		return (X * 397) ^ Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Point left, Point right)
	{
		return left.Equals(ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Point left, Point right)
	{
		return !left.Equals(ref right);
	}

	public override string ToString()
	{
		return $"({X},{Y})";
	}

	public static explicit operator Point(Vector2 value)
	{
		return new Point((int)value.X, (int)value.Y);
	}

	public static implicit operator Vector2(Point value)
	{
		return new Vector2(value.X, value.Y);
	}

	public unsafe static implicit operator RawPoint(Point value)
	{
		return *(RawPoint*)(&value);
	}

	public unsafe static implicit operator Point(RawPoint value)
	{
		return *(Point*)(&value);
	}
}
