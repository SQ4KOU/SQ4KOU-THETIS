using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Bool4 : IEquatable<Bool4>, IFormattable
{
	public static readonly int SizeInBytes = Utilities.SizeOf<Bool4>();

	public static readonly Bool4 False = default(Bool4);

	public static readonly Bool4 UnitX = new Bool4(x: true, y: false, z: false, w: false);

	public static readonly Bool4 UnitY = new Bool4(x: false, y: true, z: false, w: false);

	public static readonly Bool4 UnitZ = new Bool4(x: false, y: false, z: true, w: false);

	public static readonly Bool4 UnitW = new Bool4(x: false, y: false, z: false, w: true);

	public static readonly Bool4 One = new Bool4(x: true, y: true, z: true, w: true);

	internal int iX;

	internal int iY;

	internal int iZ;

	internal int iW;

	public bool X
	{
		get
		{
			return iX != 0;
		}
		set
		{
			iX = (value ? 1 : 0);
		}
	}

	public bool Y
	{
		get
		{
			return iY != 0;
		}
		set
		{
			iY = (value ? 1 : 0);
		}
	}

	public bool Z
	{
		get
		{
			return iZ != 0;
		}
		set
		{
			iZ = (value ? 1 : 0);
		}
	}

	public bool W
	{
		get
		{
			return iW != 0;
		}
		set
		{
			iW = (value ? 1 : 0);
		}
	}

	public bool this[int index]
	{
		get
		{
			return index switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
				3 => W, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Bool4 run from 0 to 3, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				X = value;
				break;
			case 1:
				Y = value;
				break;
			case 2:
				Z = value;
				break;
			case 3:
				W = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Bool4 run from 0 to 3, inclusive.");
			}
		}
	}

	public Bool4(bool value)
	{
		iX = (value ? 1 : 0);
		iY = (value ? 1 : 0);
		iZ = (value ? 1 : 0);
		iW = (value ? 1 : 0);
	}

	public Bool4(bool x, bool y, bool z, bool w)
	{
		iX = (x ? 1 : 0);
		iY = (y ? 1 : 0);
		iZ = (z ? 1 : 0);
		iW = (w ? 1 : 0);
	}

	public Bool4(bool[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Bool4.");
		}
		iX = (values[0] ? 1 : 0);
		iY = (values[1] ? 1 : 0);
		iZ = (values[2] ? 1 : 0);
		iW = (values[3] ? 1 : 0);
	}

	public bool[] ToArray()
	{
		return new bool[4] { X, Y, Z, W };
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Bool4 left, Bool4 right)
	{
		return left.Equals(ref right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Bool4 left, Bool4 right)
	{
		return !left.Equals(ref right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, format, X, Y, Z, W);
	}

	public override int GetHashCode()
	{
		return (((((iX * 397) ^ iY) * 397) ^ iZ) * 397) ^ iW;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ref Bool4 other)
	{
		if (other.X == X && other.Y == Y && other.Z == Z)
		{
			return other.W == W;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Bool4 other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object value)
	{
		if (!(value is Bool4 other))
		{
			return false;
		}
		return Equals(ref other);
	}

	public static implicit operator Bool4(bool[] input)
	{
		return new Bool4(input);
	}

	public static implicit operator bool[](Bool4 input)
	{
		return input.ToArray();
	}

	public unsafe static implicit operator RawBool4(Bool4 value)
	{
		return *(RawBool4*)(&value);
	}

	public unsafe static implicit operator Bool4(RawBool4 value)
	{
		return *(Bool4*)(&value);
	}
}
