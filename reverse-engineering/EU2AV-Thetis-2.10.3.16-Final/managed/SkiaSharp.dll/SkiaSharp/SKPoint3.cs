using System;
using System.Numerics;

namespace SkiaSharp;

public struct SKPoint3(float x, float y, float z) : IEquatable<SKPoint3>
{
	public static readonly SKPoint3 Empty;

	private float x = x;

	private float y = y;

	private float z = z;

	public readonly bool IsEmpty => this == Empty;

	public float X
	{
		readonly get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		readonly get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public float Z
	{
		readonly get
		{
			return z;
		}
		set
		{
			z = value;
		}
	}

	public override readonly string ToString()
	{
		return $"{{X={x}, Y={y}, Z={z}}}";
	}

	public static SKPoint3 Add(SKPoint3 pt, SKPoint3 sz)
	{
		return pt + sz;
	}

	public static SKPoint3 Subtract(SKPoint3 pt, SKPoint3 sz)
	{
		return pt - sz;
	}

	public static SKPoint3 operator +(SKPoint3 pt, SKPoint3 sz)
	{
		return new SKPoint3(pt.X + sz.X, pt.Y + sz.Y, pt.Z + sz.Z);
	}

	public static SKPoint3 operator -(SKPoint3 pt, SKPoint3 sz)
	{
		return new SKPoint3(pt.X - sz.X, pt.Y - sz.Y, pt.Z - sz.Z);
	}

	public static implicit operator Vector3(SKPoint3 point)
	{
		return new Vector3(point.x, point.y, point.z);
	}

	public static implicit operator SKPoint3(Vector3 vector)
	{
		return new SKPoint3(vector.X, vector.Y, vector.Z);
	}

	public readonly bool Equals(SKPoint3 obj)
	{
		if (x == obj.x && y == obj.y)
		{
			return z == obj.z;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKPoint3 obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKPoint3 left, SKPoint3 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKPoint3 left, SKPoint3 right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(x);
		hashCode.Add(y);
		hashCode.Add(z);
		return hashCode.ToHashCode();
	}
}
