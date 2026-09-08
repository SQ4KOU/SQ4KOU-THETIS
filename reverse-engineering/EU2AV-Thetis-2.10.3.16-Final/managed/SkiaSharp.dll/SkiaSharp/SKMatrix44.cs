using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SkiaSharp;

public struct SKMatrix44 : IEquatable<SKMatrix44>
{
	private float m00;

	private float m01;

	private float m02;

	private float m03;

	private float m10;

	private float m11;

	private float m12;

	private float m13;

	private float m20;

	private float m21;

	private float m22;

	private float m23;

	private float m30;

	private float m31;

	private float m32;

	private float m33;

	internal const float DegreesToRadians = (float)Math.PI / 180f;

	public static readonly SKMatrix44 Empty;

	public static readonly SKMatrix44 Identity = Matrix4x4.Identity;

	public float M00
	{
		readonly get
		{
			return m00;
		}
		set
		{
			m00 = value;
		}
	}

	public float M01
	{
		readonly get
		{
			return m01;
		}
		set
		{
			m01 = value;
		}
	}

	public float M02
	{
		readonly get
		{
			return m02;
		}
		set
		{
			m02 = value;
		}
	}

	public float M03
	{
		readonly get
		{
			return m03;
		}
		set
		{
			m03 = value;
		}
	}

	public float M10
	{
		readonly get
		{
			return m10;
		}
		set
		{
			m10 = value;
		}
	}

	public float M11
	{
		readonly get
		{
			return m11;
		}
		set
		{
			m11 = value;
		}
	}

	public float M12
	{
		readonly get
		{
			return m12;
		}
		set
		{
			m12 = value;
		}
	}

	public float M13
	{
		readonly get
		{
			return m13;
		}
		set
		{
			m13 = value;
		}
	}

	public float M20
	{
		readonly get
		{
			return m20;
		}
		set
		{
			m20 = value;
		}
	}

	public float M21
	{
		readonly get
		{
			return m21;
		}
		set
		{
			m21 = value;
		}
	}

	public float M22
	{
		readonly get
		{
			return m22;
		}
		set
		{
			m22 = value;
		}
	}

	public float M23
	{
		readonly get
		{
			return m23;
		}
		set
		{
			m23 = value;
		}
	}

	public float M30
	{
		readonly get
		{
			return m30;
		}
		set
		{
			m30 = value;
		}
	}

	public float M31
	{
		readonly get
		{
			return m31;
		}
		set
		{
			m31 = value;
		}
	}

	public float M32
	{
		readonly get
		{
			return m32;
		}
		set
		{
			m32 = value;
		}
	}

	public float M33
	{
		readonly get
		{
			return m33;
		}
		set
		{
			m33 = value;
		}
	}

	public readonly bool IsInvertible
	{
		get
		{
			Matrix4x4 result;
			return Matrix4x4.Invert(this, out result);
		}
	}

	public SKMatrix Matrix => new SKMatrix(m00, m10, m30, m01, m11, m31, m03, m13, m33);

	public float this[int row, int column]
	{
		get
		{
			return row switch
			{
				0 => column switch
				{
					0 => m00, 
					1 => m01, 
					2 => m02, 
					3 => m03, 
					_ => throw new ArgumentOutOfRangeException("column"), 
				}, 
				1 => column switch
				{
					0 => m10, 
					1 => m11, 
					2 => m12, 
					3 => m13, 
					_ => throw new ArgumentOutOfRangeException("column"), 
				}, 
				2 => column switch
				{
					0 => m20, 
					1 => m21, 
					2 => m22, 
					3 => m23, 
					_ => throw new ArgumentOutOfRangeException("column"), 
				}, 
				3 => column switch
				{
					0 => m30, 
					1 => m31, 
					2 => m32, 
					3 => m33, 
					_ => throw new ArgumentOutOfRangeException("column"), 
				}, 
				_ => throw new ArgumentOutOfRangeException("row"), 
			};
		}
		set
		{
			switch (row)
			{
			case 0:
				switch (column)
				{
				case 0:
					m00 = value;
					break;
				case 1:
					m01 = value;
					break;
				case 2:
					m02 = value;
					break;
				case 3:
					m03 = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("column");
				}
				break;
			case 1:
				switch (column)
				{
				case 0:
					m10 = value;
					break;
				case 1:
					m11 = value;
					break;
				case 2:
					m12 = value;
					break;
				case 3:
					m13 = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("column");
				}
				break;
			case 2:
				switch (column)
				{
				case 0:
					m20 = value;
					break;
				case 1:
					m21 = value;
					break;
				case 2:
					m22 = value;
					break;
				case 3:
					m23 = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("column");
				}
				break;
			case 3:
				switch (column)
				{
				case 0:
					m30 = value;
					break;
				case 1:
					m31 = value;
					break;
				case 2:
					m32 = value;
					break;
				case 3:
					m33 = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("column");
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("row");
			}
		}
	}

	public readonly bool Equals(SKMatrix44 obj)
	{
		if (m00 == obj.m00 && m01 == obj.m01 && m02 == obj.m02 && m03 == obj.m03 && m10 == obj.m10 && m11 == obj.m11 && m12 == obj.m12 && m13 == obj.m13 && m20 == obj.m20 && m21 == obj.m21 && m22 == obj.m22 && m23 == obj.m23 && m30 == obj.m30 && m31 == obj.m31 && m32 == obj.m32)
		{
			return m33 == obj.m33;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKMatrix44 obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKMatrix44 left, SKMatrix44 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKMatrix44 left, SKMatrix44 right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(m00);
		hashCode.Add(m01);
		hashCode.Add(m02);
		hashCode.Add(m03);
		hashCode.Add(m10);
		hashCode.Add(m11);
		hashCode.Add(m12);
		hashCode.Add(m13);
		hashCode.Add(m20);
		hashCode.Add(m21);
		hashCode.Add(m22);
		hashCode.Add(m23);
		hashCode.Add(m30);
		hashCode.Add(m31);
		hashCode.Add(m32);
		hashCode.Add(m33);
		return hashCode.ToHashCode();
	}

	public SKMatrix44()
	{
		m00 = 0f;
		m01 = 0f;
		m02 = 0f;
		m03 = 0f;
		m10 = 0f;
		m11 = 0f;
		m12 = 0f;
		m13 = 0f;
		m20 = 0f;
		m21 = 0f;
		m22 = 0f;
		m23 = 0f;
		m30 = 0f;
		m31 = 0f;
		m32 = 0f;
		m33 = 0f;
	}

	public SKMatrix44(SKMatrix src)
	{
		this = src;
	}

	public SKMatrix44(SKMatrix44 src)
	{
		this = src;
	}

	public SKMatrix44(float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33)
	{
		this.m00 = m00;
		this.m01 = m01;
		this.m02 = m02;
		this.m03 = m03;
		this.m10 = m10;
		this.m11 = m11;
		this.m12 = m12;
		this.m13 = m13;
		this.m20 = m20;
		this.m21 = m21;
		this.m22 = m22;
		this.m23 = m23;
		this.m30 = m30;
		this.m31 = m31;
		this.m32 = m32;
		this.m33 = m33;
	}

	public static SKMatrix44 CreateIdentity()
	{
		return Identity;
	}

	public static SKMatrix44 CreateTranslation(float x, float y, float z)
	{
		return Matrix4x4.CreateTranslation(x, y, z);
	}

	public static SKMatrix44 CreateScale(float x, float y, float z)
	{
		return Matrix4x4.CreateScale(x, y, z);
	}

	public static SKMatrix44 CreateScale(float x, float y, float z, float pivotX, float pivotY, float pivotZ)
	{
		return Matrix4x4.CreateScale(x, y, z, new Vector3(pivotX, pivotY, pivotZ));
	}

	public static SKMatrix44 CreateRotation(float x, float y, float z, float radians)
	{
		return Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z), radians);
	}

	public static SKMatrix44 CreateRotationDegrees(float x, float y, float z, float degrees)
	{
		return Matrix4x4.CreateFromAxisAngle(new Vector3(x, y, z), degrees * ((float)Math.PI / 180f));
	}

	public static SKMatrix44 FromRowMajor(ReadOnlySpan<float> src)
	{
		if (src.Length != 16)
		{
			throw new ArgumentException("The source array must be 16 entries.", "src");
		}
		return new SKMatrix44(src[0], src[1], src[2], src[3], src[4], src[5], src[6], src[7], src[8], src[9], src[10], src[11], src[12], src[13], src[14], src[15]);
	}

	public static SKMatrix44 FromColumnMajor(ReadOnlySpan<float> src)
	{
		if (src.Length != 16)
		{
			throw new ArgumentException("The source array must be 16 entries.", "src");
		}
		return new SKMatrix44(src[0], src[4], src[8], src[12], src[1], src[5], src[9], src[13], src[2], src[6], src[10], src[14], src[3], src[7], src[11], src[15]);
	}

	public readonly float[] ToRowMajor()
	{
		float[] array = new float[16];
		ToRowMajor(array);
		return array;
	}

	public readonly float[] ToColumnMajor()
	{
		float[] array = new float[16];
		ToColumnMajor(array);
		return array;
	}

	public readonly void ToRowMajor(Span<float> dst)
	{
		if (dst.Length != 16)
		{
			throw new ArgumentException("The destination array must be 16 entries.", "dst");
		}
		dst[0] = m00;
		dst[1] = m01;
		dst[2] = m02;
		dst[3] = m03;
		dst[4] = m10;
		dst[5] = m11;
		dst[6] = m12;
		dst[7] = m13;
		dst[8] = m20;
		dst[9] = m21;
		dst[10] = m22;
		dst[11] = m23;
		dst[12] = m30;
		dst[13] = m31;
		dst[14] = m32;
		dst[15] = m33;
	}

	public readonly void ToColumnMajor(Span<float> dst)
	{
		if (dst.Length != 16)
		{
			throw new ArgumentException("The destination array must be 16 entries.", "dst");
		}
		dst[0] = m00;
		dst[1] = m10;
		dst[2] = m20;
		dst[3] = m30;
		dst[4] = m01;
		dst[5] = m11;
		dst[6] = m21;
		dst[7] = m31;
		dst[8] = m02;
		dst[9] = m12;
		dst[10] = m22;
		dst[11] = m32;
		dst[12] = m03;
		dst[13] = m13;
		dst[14] = m23;
		dst[15] = m33;
	}

	public readonly bool TryInvert(out SKMatrix44 inverse)
	{
		if (Matrix4x4.Invert(this, out var result))
		{
			inverse = result;
			return true;
		}
		inverse = Empty;
		return false;
	}

	public readonly SKMatrix44 Invert()
	{
		if (Matrix4x4.Invert(this, out var result))
		{
			return result;
		}
		return Empty;
	}

	public readonly SKMatrix44 Transpose()
	{
		return Matrix4x4.Transpose(this);
	}

	public readonly float Determinant()
	{
		return ((Matrix4x4)this).GetDeterminant();
	}

	public readonly SKPoint MapPoint(SKPoint point)
	{
		return Vector2.Transform(point, this);
	}

	public readonly SKPoint3 MapPoint(SKPoint3 point)
	{
		return Vector3.Transform(point, this);
	}

	public readonly SKPoint MapPoint(float x, float y)
	{
		return MapPoint(new SKPoint(x, y));
	}

	public readonly SKPoint3 MapPoint(float x, float y, float z)
	{
		return MapPoint(new SKPoint3(x, y, z));
	}

	internal float[] MapScalars(float x, float y, float z, float w)
	{
		Vector4 vector = Vector4.Transform(new Vector4(x, y, z, w), this);
		return new float[4] { vector.X, vector.Y, vector.Z, vector.W };
	}

	internal float[] MapScalars(ReadOnlySpan<float> srcVector4)
	{
		if (srcVector4 == null)
		{
			throw new ArgumentNullException("srcVector4");
		}
		if (srcVector4.Length != 4)
		{
			throw new ArgumentException("The source vector array must be 4 entries.", "srcVector4");
		}
		Vector4 vector = new Vector4(srcVector4[0], srcVector4[1], srcVector4[2], srcVector4[3]);
		Vector4 vector2 = Vector4.Transform(vector, this);
		return new float[4] { vector2.X, vector2.Y, vector2.Z, vector2.W };
	}

	internal void MapScalars(ReadOnlySpan<float> srcVector4, Span<float> dstVector4)
	{
		if (srcVector4 == null)
		{
			throw new ArgumentNullException("srcVector4");
		}
		if (srcVector4.Length != 4)
		{
			throw new ArgumentException("The source vector array must be 4 entries.", "srcVector4");
		}
		if (dstVector4 == null)
		{
			throw new ArgumentNullException("dstVector4");
		}
		if (dstVector4.Length != 4)
		{
			throw new ArgumentException("The destination vector array must be 4 entries.", "dstVector4");
		}
		Vector4 vector = new Vector4(srcVector4[0], srcVector4[1], srcVector4[2], srcVector4[3]);
		Vector4 vector2 = Vector4.Transform(vector, this);
		dstVector4[0] = vector2.X;
		dstVector4[1] = vector2.Y;
		dstVector4[2] = vector2.Z;
		dstVector4[3] = vector2.W;
	}

	public static SKMatrix44 Concat(SKMatrix44 first, SKMatrix44 second)
	{
		return first * second;
	}

	public readonly SKMatrix44 PreConcat(SKMatrix44 matrix)
	{
		return this * matrix;
	}

	public readonly SKMatrix44 PostConcat(SKMatrix44 matrix)
	{
		return matrix * this;
	}

	public static void Concat(ref SKMatrix44 target, SKMatrix44 first, SKMatrix44 second)
	{
		target = first * second;
	}

	public static SKMatrix44 Negate(SKMatrix44 value)
	{
		return -value;
	}

	public static SKMatrix44 Add(SKMatrix44 value1, SKMatrix44 value2)
	{
		return value1 + value2;
	}

	public static SKMatrix44 Subtract(SKMatrix44 value1, SKMatrix44 value2)
	{
		return value1 - value2;
	}

	public static SKMatrix44 Multiply(SKMatrix44 value1, SKMatrix44 value2)
	{
		return value1 * value2;
	}

	public static SKMatrix44 Multiply(SKMatrix44 value1, float value2)
	{
		return value1 * value2;
	}

	public static SKMatrix44 operator -(SKMatrix44 value)
	{
		return -(Matrix4x4)value;
	}

	public static SKMatrix44 operator +(SKMatrix44 value1, SKMatrix44 value2)
	{
		return (Matrix4x4)value1 + (Matrix4x4)value2;
	}

	public static SKMatrix44 operator -(SKMatrix44 value1, SKMatrix44 value2)
	{
		return (Matrix4x4)value1 - (Matrix4x4)value2;
	}

	public static SKMatrix44 operator *(SKMatrix44 value1, SKMatrix44 value2)
	{
		return (Matrix4x4)value1 * (Matrix4x4)value2;
	}

	public static SKMatrix44 operator *(SKMatrix44 value1, float value2)
	{
		return (Matrix4x4)value1 * value2;
	}

	public static implicit operator SKMatrix44(SKMatrix matrix)
	{
		return new SKMatrix44(matrix.ScaleX, matrix.SkewY, 0f, matrix.Persp0, matrix.SkewX, matrix.ScaleY, 0f, matrix.Persp1, 0f, 0f, 1f, 0f, matrix.TransX, matrix.TransY, 0f, matrix.Persp2);
	}

	public static implicit operator Matrix4x4(SKMatrix44 matrix)
	{
		return Unsafe.As<SKMatrix44, Matrix4x4>(ref matrix);
	}

	public static implicit operator SKMatrix44(Matrix4x4 matrix)
	{
		return Unsafe.As<Matrix4x4, SKMatrix44>(ref matrix);
	}
}
