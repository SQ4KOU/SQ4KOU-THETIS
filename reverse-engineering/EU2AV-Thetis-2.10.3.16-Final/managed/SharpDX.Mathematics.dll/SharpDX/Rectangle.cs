using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

public struct Rectangle : IEquatable<Rectangle>
{
	public int Left;

	public int Top;

	public int Right;

	public int Bottom;

	public static readonly Rectangle Empty;

	public int X
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

	public int Y
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

	public int Width
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

	public int Height
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

	public bool IsEmpty
	{
		get
		{
			if (Width == 0 && Height == 0 && X == 0)
			{
				return Y == 0;
			}
			return false;
		}
	}

	static Rectangle()
	{
		Empty = default(Rectangle);
	}

	public Rectangle(int x, int y, int width, int height)
	{
		Left = x;
		Top = y;
		Right = x + width;
		Bottom = y + height;
	}

	public void Offset(int offsetX, int offsetY)
	{
		X += offsetX;
		Y += offsetY;
	}

	public void Inflate(int horizontalAmount, int verticalAmount)
	{
		X -= horizontalAmount;
		Y -= verticalAmount;
		Width += horizontalAmount * 2;
		Height += verticalAmount * 2;
	}

	public bool Contains(int x, int y)
	{
		if (X <= x && x < Right && Y <= y)
		{
			return y < Bottom;
		}
		return false;
	}

	public bool Contains(Rectangle value)
	{
		Contains(ref value, out var result);
		return result;
	}

	public void Contains(ref Rectangle value, out bool result)
	{
		result = X <= value.X && value.Right <= Right && Y <= value.Y && value.Bottom <= Bottom;
	}

	public bool Contains(float x, float y)
	{
		if (x >= (float)Left && x <= (float)Right && y >= (float)Top)
		{
			return y <= (float)Bottom;
		}
		return false;
	}

	public bool Intersects(Rectangle value)
	{
		Intersects(ref value, out var result);
		return result;
	}

	public void Intersects(ref Rectangle value, out bool result)
	{
		result = value.X < Right && X < value.Right && value.Y < Bottom && Y < value.Bottom;
	}

	public static Rectangle Intersect(Rectangle value1, Rectangle value2)
	{
		Intersect(ref value1, ref value2, out var result);
		return result;
	}

	public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		int num = ((value1.X > value2.X) ? value1.X : value2.X);
		int num2 = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		int num3 = ((value1.Right < value2.Right) ? value1.Right : value2.Right);
		int num4 = ((value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom);
		if (num3 > num && num4 > num2)
		{
			result = new Rectangle(num, num2, num3 - num, num4 - num2);
		}
		else
		{
			result = Empty;
		}
	}

	public static Rectangle Union(Rectangle value1, Rectangle value2)
	{
		Union(ref value1, ref value2, out var result);
		return result;
	}

	public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		int num = Math.Min(value1.Left, value2.Left);
		int num2 = Math.Max(value1.Right, value2.Right);
		int num3 = Math.Min(value1.Top, value2.Top);
		int num4 = Math.Max(value1.Bottom, value2.Bottom);
		result = new Rectangle(num, num3, num2 - num, num4 - num3);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Rectangle other))
		{
			return false;
		}
		return Equals(ref other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ref Rectangle other)
	{
		if (other.Left == Left && other.Top == Top && other.Right == Right)
		{
			return other.Bottom == Bottom;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Rectangle other)
	{
		return Equals(ref other);
	}

	public override int GetHashCode()
	{
		return (((((Left * 397) ^ Top) * 397) ^ Right) * 397) ^ Bottom;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Rectangle left, Rectangle right)
	{
		return left.Equals(ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Rectangle left, Rectangle right)
	{
		return !left.Equals(ref right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
	}

	internal void MakeXYAndWidthHeight()
	{
		Right -= Left;
		Bottom -= Top;
	}

	public unsafe static implicit operator RawRectangle(Rectangle value)
	{
		return *(RawRectangle*)(&value);
	}

	public unsafe static implicit operator Rectangle(RawRectangle value)
	{
		return *(Rectangle*)(&value);
	}

	public static implicit operator RawBox(Rectangle value)
	{
		return new RawBox(value.X, value.Y, value.Width, value.Height);
	}

	public static implicit operator Rectangle(RawBox value)
	{
		return new Rectangle(value.X, value.Y, value.Width, value.Height);
	}
}
