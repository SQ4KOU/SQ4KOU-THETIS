using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX;

public struct Half3 : IEquatable<Half3>
{
	public Half X;

	public Half Y;

	public Half Z;

	public Half3(Half x, Half y, Half z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Half3(float x, float y, float z)
	{
		X = new Half(x);
		Y = new Half(y);
		Z = new Half(z);
	}

	public Half3(ushort x, ushort y, ushort z)
	{
		X = new Half(x);
		Y = new Half(y);
		Z = new Half(z);
	}

	public Half3(Half value)
	{
		X = value;
		Y = value;
		Z = value;
	}

	public static implicit operator Half3(Vector3 value)
	{
		return new Half3(value.X, value.Y, value.Z);
	}

	public static implicit operator Vector3(Half3 value)
	{
		return new Vector3(value.X, value.Y, value.Z);
	}

	public static explicit operator Half3(Vector2 value)
	{
		return new Half3(value.X, value.Y, 0f);
	}

	public static explicit operator Vector2(Half3 value)
	{
		return new Vector2(value.X, value.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Half3 left, Half3 right)
	{
		return Equals(ref left, ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: MarshalAs(UnmanagedType.U1)]
	public static bool operator !=(Half3 left, Half3 right)
	{
		return !Equals(ref left, ref right);
	}

	public override int GetHashCode()
	{
		return (((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Equals(ref Half3 value1, ref Half3 value2)
	{
		if (value1.X == value2.X && value1.Y == value2.Y)
		{
			return value1.Z == value2.Z;
		}
		return false;
	}

	public bool Equals(Half3 other)
	{
		if (X == other.X && Y == other.Y)
		{
			return Z == other.Z;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Half3))
		{
			return false;
		}
		return Equals((Half3)obj);
	}
}
