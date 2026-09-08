using System;
using System.Globalization;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;

internal class CSharpPrimitiveFormatter : CommonPrimitiveFormatter
{
	protected override string NullLiteral => ObjectDisplay.NullLiteral;

	protected override string FormatLiteral(bool value)
	{
		return ObjectDisplay.FormatLiteral(value);
	}

	protected override string FormatLiteral(string value, bool useQuotes, bool escapeNonPrintable, int numberRadix = 10)
	{
		ObjectDisplayOptions objectDisplayOptions = ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes, escapeNonPrintable, includeCodePoints: false, numberRadix);
		return ObjectDisplay.FormatLiteral(value, objectDisplayOptions);
	}

	protected override string FormatLiteral(char c, bool useQuotes, bool escapeNonPrintable, bool includeCodePoints = false, int numberRadix = 10)
	{
		ObjectDisplayOptions objectDisplayOptions = ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes, escapeNonPrintable, includeCodePoints, numberRadix);
		return ObjectDisplay.FormatLiteral(c, objectDisplayOptions);
	}

	protected override string FormatLiteral(sbyte value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(byte value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(short value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(ushort value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(int value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(uint value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(long value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(ulong value, int numberRadix = 10, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectFormatterHelpers.GetObjectDisplayOptions(useQuotes: false, escapeNonPrintable: false, includeCodePoints: false, numberRadix), cultureInfo);
	}

	protected override string FormatLiteral(double value, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None, cultureInfo);
	}

	protected override string FormatLiteral(float value, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None, cultureInfo);
	}

	protected override string FormatLiteral(decimal value, CultureInfo cultureInfo = null)
	{
		return ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None, cultureInfo);
	}

	protected override string FormatLiteral(DateTime value, CultureInfo cultureInfo = null)
	{
		return null;
	}
}
