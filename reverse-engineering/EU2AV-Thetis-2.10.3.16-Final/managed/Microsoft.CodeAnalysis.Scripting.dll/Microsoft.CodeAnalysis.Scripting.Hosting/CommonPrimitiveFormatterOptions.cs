using System.Globalization;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal readonly struct CommonPrimitiveFormatterOptions
{
	public int NumberRadix { get; }

	public bool IncludeCharacterCodePoints { get; }

	public bool QuoteStringsAndCharacters { get; }

	public bool EscapeNonPrintableCharacters { get; }

	public CultureInfo CultureInfo { get; }

	public CommonPrimitiveFormatterOptions(int numberRadix, bool includeCodePoints, bool quoteStringsAndCharacters, bool escapeNonPrintableCharacters, CultureInfo cultureInfo)
	{
		NumberRadix = numberRadix;
		IncludeCharacterCodePoints = includeCodePoints;
		QuoteStringsAndCharacters = quoteStringsAndCharacters;
		EscapeNonPrintableCharacters = escapeNonPrintableCharacters;
		CultureInfo = cultureInfo;
	}
}
