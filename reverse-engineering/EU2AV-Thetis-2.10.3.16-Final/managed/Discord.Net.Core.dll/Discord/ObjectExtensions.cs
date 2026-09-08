using System;

namespace Discord;

internal static class ObjectExtensions
{
	private const long Int53Max = 9007199254740991L;

	private const long Int53Min = -9007199254740991L;

	public static (double Min, double Max) GetSupportedNumericalRange(this object o)
	{
		return Type.GetTypeCode(o.GetType()) switch
		{
			TypeCode.Byte => (Min: 0.0, Max: 255.0), 
			TypeCode.SByte => (Min: -128.0, Max: 127.0), 
			TypeCode.Int16 => (Min: -32768.0, Max: 32767.0), 
			TypeCode.UInt16 => (Min: 0.0, Max: 65535.0), 
			TypeCode.Int32 => (Min: -2147483648.0, Max: 2147483647.0), 
			TypeCode.UInt32 => (Min: 0.0, Max: 4294967295.0), 
			TypeCode.UInt64 => (Min: 0.0, Max: 9007199254740991.0), 
			_ => (Min: -9007199254740991.0, Max: 9007199254740991.0), 
		};
	}

	public static bool IsNumericType(this object o)
	{
		TypeCode typeCode = Type.GetTypeCode(o.GetType());
		if ((uint)(typeCode - 5) <= 10u)
		{
			return true;
		}
		return false;
	}
}
