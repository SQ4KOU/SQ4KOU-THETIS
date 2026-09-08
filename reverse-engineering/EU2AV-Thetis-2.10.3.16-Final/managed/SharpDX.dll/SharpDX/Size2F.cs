using System;

namespace SharpDX;

public struct Size2F(float width, float height) : IEquatable<Size2F>
{
	public static readonly Size2F Zero = new Size2F(0f, 0f);

	public static readonly Size2F Empty = Zero;

	public float Width = width;

	public float Height = height;

	public bool Equals(Size2F other)
	{
		if (other.Width == Width)
		{
			return other.Height == Height;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Size2F))
		{
			return false;
		}
		return Equals((Size2F)obj);
	}

	public override int GetHashCode()
	{
		return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
	}

	public static bool operator ==(Size2F left, Size2F right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Size2F left, Size2F right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"({Width},{Height})";
	}
}
