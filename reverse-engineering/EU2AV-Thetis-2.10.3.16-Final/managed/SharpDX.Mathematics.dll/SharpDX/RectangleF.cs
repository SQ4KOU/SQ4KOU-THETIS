using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

public struct RectangleF : IEquatable<RectangleF>
{
	public float Left;

	public float Top;

	public float Right;

	public float Bottom;

	public static readonly RectangleF Empty;

	public static readonly RectangleF Infinite;

	public float X
	{
		get
		{
			return Left;
		}
		set
		{
			Right = value + Width;
			Left = value;
		}
	}

	public float Y
	{
		get
		{
			return Top;
		}
		set
		{
			Bottom = value + Height;
			Top = value;
		}
	}

	public float Width
	{
		get
		{
			return Right - Left;
		}
		set
		{
			Right = Left + value;
		}
	}

	public float Height
	{
		get
		{
			return Bottom - Top;
		}
		set
		{
			Bottom = Top + value;
		}
	}

	public Vector2 Location
	{
		get
		{
			return new Vector2(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

	public bool IsEmpty
	{
		get
		{
			if (Width == 0f && Height == 0f && X == 0f)
			{
				return Y == 0f;
			}
			return false;
		}
	}

	public Size2F Size
	{
		get
		{
			return new Size2F(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	public Vector2 TopLeft => new Vector2(Left, Top);

	public Vector2 TopRight => new Vector2(Right, Top);

	public Vector2 BottomLeft => new Vector2(Left, Bottom);

	public Vector2 BottomRight => new Vector2(Right, Bottom);

	static RectangleF()
	{
		Empty = default(RectangleF);
		Infinite = new RectangleF
		{
			Left = float.NegativeInfinity,
			Top = float.NegativeInfinity,
			Right = float.PositiveInfinity,
			Bottom = float.PositiveInfinity
		};
	}

	public RectangleF(float x, float y, float width, float height)
	{
		Left = x;
		Top = y;
		Right = x + width;
		Bottom = y + height;
	}

	public void Offset(Point amount)
	{
		Offset(amount.X, amount.Y);
	}

	public void Offset(Vector2 amount)
	{
		Offset(amount.X, amount.Y);
	}

	public void Offset(float offsetX, float offsetY)
	{
		X += offsetX;
		Y += offsetY;
	}

	public void Inflate(float horizontalAmount, float verticalAmount)
	{
		X -= horizontalAmount;
		Y -= verticalAmount;
		Width += horizontalAmount * 2f;
		Height += verticalAmount * 2f;
	}

	public void Contains(ref Vector2 value, out bool result)
	{
		result = value.X >= Left && value.X <= Right && value.Y >= Top && value.Y <= Bottom;
	}

	public bool Contains(Rectangle value)
	{
		if (X <= (float)value.X && (float)value.Right <= Right && Y <= (float)value.Y)
		{
			return (float)value.Bottom <= Bottom;
		}
		return false;
	}

	public void Contains(ref RectangleF value, out bool result)
	{
		result = X <= value.X && value.Right <= Right && Y <= value.Y && value.Bottom <= Bottom;
	}

	public bool Contains(float x, float y)
	{
		if (x >= Left && x <= Right && y >= Top)
		{
			return y <= Bottom;
		}
		return false;
	}

	public bool Contains(Vector2 vector2D)
	{
		return Contains(vector2D.X, vector2D.Y);
	}

	public bool Contains(Point point)
	{
		return Contains(point.X, point.Y);
	}

	public bool Intersects(RectangleF value)
	{
		Intersects(ref value, out var result);
		return result;
	}

	public void Intersects(ref RectangleF value, out bool result)
	{
		result = value.X < Right && X < value.Right && value.Y < Bottom && Y < value.Bottom;
	}

	public static RectangleF Intersect(RectangleF value1, RectangleF value2)
	{
		Intersect(ref value1, ref value2, out var result);
		return result;
	}

	public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
	{
		float num = ((value1.X > value2.X) ? value1.X : value2.X);
		float num2 = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		float num3 = ((value1.Right < value2.Right) ? value1.Right : value2.Right);
		float num4 = ((value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom);
		if (num3 > num && num4 > num2)
		{
			result = new RectangleF(num, num2, num3 - num, num4 - num2);
		}
		else
		{
			result = Empty;
		}
	}

	public static RectangleF Union(RectangleF value1, RectangleF value2)
	{
		Union(ref value1, ref value2, out var result);
		return result;
	}

	public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
	{
		float num = Math.Min(value1.Left, value2.Left);
		float num2 = Math.Max(value1.Right, value2.Right);
		float num3 = Math.Min(value1.Top, value2.Top);
		float num4 = Math.Max(value1.Bottom, value2.Bottom);
		result = new RectangleF(num, num3, num2 - num, num4 - num3);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RectangleF other))
		{
			return false;
		}
		return Equals(ref other);
	}

	public bool Equals(ref RectangleF other)
	{
		if (MathUtil.NearEqual(other.Left, Left) && MathUtil.NearEqual(other.Right, Right) && MathUtil.NearEqual(other.Top, Top))
		{
			return MathUtil.NearEqual(other.Bottom, Bottom);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(RectangleF other)
	{
		return Equals(ref other);
	}

	public override int GetHashCode()
	{
		return (((((Left.GetHashCode() * 397) ^ Top.GetHashCode()) * 397) ^ Right.GetHashCode()) * 397) ^ Bottom.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(RectangleF left, RectangleF right)
	{
		return left.Equals(ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(RectangleF left, RectangleF right)
	{
		return !left.Equals(ref right);
	}

	public static explicit operator Rectangle(RectangleF value)
	{
		return new Rectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
	}

	public static implicit operator RawRectangle(RectangleF value)
	{
		return new RawRectangle((int)value.X, (int)value.Y, (int)value.Right, (int)value.Bottom);
	}

	public static implicit operator RawRectangleF(RectangleF value)
	{
		return new RawRectangleF(value.X, value.Y, value.Right, value.Bottom);
	}
}
