using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ViewportF : IEquatable<ViewportF>
{
	public float X;

	public float Y;

	public float Width;

	public float Height;

	public float MinDepth;

	public float MaxDepth;

	public RectangleF Bounds
	{
		get
		{
			return new RectangleF(X, Y, Width, Height);
		}
		set
		{
			X = value.X;
			Y = value.Y;
			Width = value.Width;
			Height = value.Height;
		}
	}

	public float AspectRatio
	{
		get
		{
			if (!MathUtil.IsZero(Height))
			{
				return Width / Height;
			}
			return 0f;
		}
	}

	public ViewportF(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		MinDepth = 0f;
		MaxDepth = 1f;
	}

	public ViewportF(float x, float y, float width, float height, float minDepth, float maxDepth)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		MinDepth = minDepth;
		MaxDepth = maxDepth;
	}

	public ViewportF(RectangleF bounds)
	{
		X = bounds.X;
		Y = bounds.Y;
		Width = bounds.Width;
		Height = bounds.Height;
		MinDepth = 0f;
		MaxDepth = 1f;
	}

	public bool Equals(ref ViewportF other)
	{
		if (MathUtil.NearEqual(X, other.X) && MathUtil.NearEqual(Y, other.Y) && MathUtil.NearEqual(Width, other.Width) && MathUtil.NearEqual(Height, other.Height) && MathUtil.NearEqual(MinDepth, other.MinDepth))
		{
			return MathUtil.NearEqual(MaxDepth, other.MaxDepth);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ViewportF other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ViewportF other))
		{
			return false;
		}
		return Equals(ref other);
	}

	public override int GetHashCode()
	{
		return (((((((((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Width.GetHashCode()) * 397) ^ Height.GetHashCode()) * 397) ^ MinDepth.GetHashCode()) * 397) ^ MaxDepth.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(ViewportF left, ViewportF right)
	{
		return left.Equals(ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(ViewportF left, ViewportF right)
	{
		return !left.Equals(ref right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3} MinDepth:{4} MaxDepth:{5}}}", X, Y, Width, Height, MinDepth, MaxDepth);
	}

	public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix.Multiply(ref world, ref view, out var result);
		Matrix.Multiply(ref result, ref projection, out result);
		Project(ref source, ref result, out var vector);
		return vector;
	}

	public void Project(ref Vector3 source, ref Matrix matrix, out Vector3 vector)
	{
		Vector3.Transform(ref source, ref matrix, out vector);
		float num = source.X * matrix.M14 + source.Y * matrix.M24 + source.Z * matrix.M34 + matrix.M44;
		if (!MathUtil.IsOne(num))
		{
			vector /= num;
		}
		vector.X = (vector.X + 1f) * 0.5f * Width + X;
		vector.Y = (0f - vector.Y + 1f) * 0.5f * Height + Y;
		vector.Z = vector.Z * (MaxDepth - MinDepth) + MinDepth;
	}

	public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix.Multiply(ref world, ref view, out var result);
		Matrix.Multiply(ref result, ref projection, out result);
		Matrix.Invert(ref result, out result);
		Unproject(ref source, ref result, out var vector);
		return vector;
	}

	public void Unproject(ref Vector3 source, ref Matrix matrix, out Vector3 vector)
	{
		vector.X = (source.X - X) / Width * 2f - 1f;
		vector.Y = 0f - ((source.Y - Y) / Height * 2f - 1f);
		vector.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);
		float num = vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44;
		Vector3.Transform(ref vector, ref matrix, out vector);
		if (!MathUtil.IsOne(num))
		{
			vector /= num;
		}
	}

	public static implicit operator ViewportF(Viewport value)
	{
		return new ViewportF(value.X, value.Y, value.Width, value.Height, value.MinDepth, value.MaxDepth);
	}

	public unsafe static implicit operator RawViewportF(ViewportF value)
	{
		return *(RawViewportF*)(&value);
	}

	public unsafe static implicit operator ViewportF(RawViewportF value)
	{
		return *(ViewportF*)(&value);
	}
}
