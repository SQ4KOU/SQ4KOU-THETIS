using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class CommonPrimitiveFormatter
{
	protected abstract string NullLiteral { get; }

	protected abstract string FormatLiteral(bool value);

	protected abstract string FormatLiteral(string value, bool quote, bool escapeNonPrintable, int numberRadix = 10);

	protected abstract string FormatLiteral(char value, bool quote, bool escapeNonPrintable, bool includeCodePoints = false, int numberRadix = 10);

	protected abstract string FormatLiteral(sbyte value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(byte value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(short value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(ushort value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(int value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(uint value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(long value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(ulong value, int numberRadix = 10, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(double value, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(float value, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(decimal value, CultureInfo cultureInfo = null);

	protected abstract string FormatLiteral(DateTime value, CultureInfo cultureInfo = null);

	public string FormatPrimitive(object obj, CommonPrimitiveFormatterOptions options)
	{
		if (obj == ObjectFormatterHelpers.VoidValue)
		{
			return string.Empty;
		}
		if (obj == null)
		{
			return NullLiteral;
		}
		Type type = obj.GetType();
		if (type.GetTypeInfo().IsEnum)
		{
			return obj.ToString();
		}
		switch (ObjectFormatterHelpers.GetPrimitiveSpecialType(type))
		{
		case SpecialType.System_Int32:
			return FormatLiteral((int)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_String:
			return FormatLiteral((string)obj, options.QuoteStringsAndCharacters, options.EscapeNonPrintableCharacters, options.NumberRadix);
		case SpecialType.System_Boolean:
			return FormatLiteral((bool)obj);
		case SpecialType.System_Char:
			return FormatLiteral((char)obj, options.QuoteStringsAndCharacters, options.EscapeNonPrintableCharacters, options.IncludeCharacterCodePoints, options.NumberRadix);
		case SpecialType.System_Int64:
			return FormatLiteral((long)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_Double:
			return FormatLiteral((double)obj, options.CultureInfo);
		case SpecialType.System_Byte:
			return FormatLiteral((byte)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_Decimal:
			return FormatLiteral((decimal)obj, options.CultureInfo);
		case SpecialType.System_UInt32:
			return FormatLiteral((uint)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_UInt64:
			return FormatLiteral((ulong)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_Single:
			return FormatLiteral((float)obj, options.CultureInfo);
		case SpecialType.System_Int16:
			return FormatLiteral((short)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_UInt16:
			return FormatLiteral((ushort)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.System_DateTime:
			return FormatLiteral((DateTime)obj, options.CultureInfo);
		case SpecialType.System_SByte:
			return FormatLiteral((sbyte)obj, options.NumberRadix, options.CultureInfo);
		case SpecialType.None:
		case SpecialType.System_Object:
		case SpecialType.System_Void:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(ObjectFormatterHelpers.GetPrimitiveSpecialType(type));
		}
	}
}
