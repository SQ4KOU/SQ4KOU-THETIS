using System.Diagnostics;

namespace System.Text;

internal static class UnicodeDebug
{
	[Conditional("DEBUG")]
	internal static void AssertIsValidCodePoint(uint codePoint)
	{
		UnicodeUtility.IsValidCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsHighSurrogateCodePoint(uint codePoint)
	{
		UnicodeUtility.IsHighSurrogateCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsLowSurrogateCodePoint(uint codePoint)
	{
		UnicodeUtility.IsLowSurrogateCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsValidScalar(uint scalarValue)
	{
		UnicodeUtility.IsValidUnicodeScalar(scalarValue);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsValidSupplementaryPlaneScalar(uint scalarValue)
	{
		if (UnicodeUtility.IsValidUnicodeScalar(scalarValue))
		{
			UnicodeUtility.IsBmpCodePoint(scalarValue);
		}
	}

	private static string ToHexString(uint codePoint)
	{
		return FormattableString.Invariant($"U+{codePoint:X4}");
	}
}
