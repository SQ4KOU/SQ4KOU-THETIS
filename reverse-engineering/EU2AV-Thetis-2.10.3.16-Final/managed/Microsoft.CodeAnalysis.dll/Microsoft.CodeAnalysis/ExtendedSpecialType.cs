using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Explicit, Size = 1)]
internal readonly struct ExtendedSpecialType
{
	[FieldOffset(0)]
	private readonly sbyte _value;

	private ExtendedSpecialType(int value)
	{
		_value = (sbyte)value;
	}

	public static implicit operator ExtendedSpecialType(SpecialType value)
	{
		return new ExtendedSpecialType((int)value);
	}

	public static explicit operator SpecialType(ExtendedSpecialType value)
	{
		if (value._value >= 47)
		{
			return SpecialType.None;
		}
		return (SpecialType)value._value;
	}

	public static implicit operator ExtendedSpecialType(InternalSpecialType value)
	{
		return new ExtendedSpecialType((int)value);
	}

	public static explicit operator InternalSpecialType(ExtendedSpecialType value)
	{
		sbyte value2 = value._value;
		if ((value2 >= 47 && value2 < 58) || 1 == 0)
		{
			return (InternalSpecialType)value._value;
		}
		return InternalSpecialType.Unknown;
	}

	public static explicit operator ExtendedSpecialType(int value)
	{
		return new ExtendedSpecialType(value);
	}

	public static explicit operator int(ExtendedSpecialType value)
	{
		return value._value;
	}

	public static bool operator ==(ExtendedSpecialType left, ExtendedSpecialType right)
	{
		return left._value == right._value;
	}

	public static bool operator !=(ExtendedSpecialType left, ExtendedSpecialType right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is ExtendedSpecialType extendedSpecialType))
		{
			if (!(obj is SpecialType specialType))
			{
				if (obj is InternalSpecialType internalSpecialType)
				{
					return this == internalSpecialType;
				}
				return false;
			}
			return this == specialType;
		}
		return this == extendedSpecialType;
	}

	public override int GetHashCode()
	{
		sbyte value = _value;
		return value.GetHashCode();
	}

	public override string ToString()
	{
		if (_value > 0 && _value <= 46)
		{
			return ((SpecialType)_value/*cast due to constrained. prefix*/).ToString();
		}
		if (_value == 47)
		{
			return "System_ReadOnlySpan_T";
		}
		if (_value > 47 && _value < 58)
		{
			return ((InternalSpecialType)_value/*cast due to constrained. prefix*/).ToString();
		}
		sbyte value = _value;
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
