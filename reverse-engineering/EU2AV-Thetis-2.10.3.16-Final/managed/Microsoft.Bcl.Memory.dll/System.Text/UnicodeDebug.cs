using System.Diagnostics;

namespace System.Text;

internal static class UnicodeDebug
{
	[Conditional("DEBUG")]
	internal static void AssertIsBmpCodePoint(uint codePoint)
	{
		System.Text.UnicodeUtility.IsBmpCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsHighSurrogateCodePoint(uint codePoint)
	{
		System.Text.UnicodeUtility.IsHighSurrogateCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsLowSurrogateCodePoint(uint codePoint)
	{
		System.Text.UnicodeUtility.IsLowSurrogateCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsValidCodePoint(uint codePoint)
	{
		System.Text.UnicodeUtility.IsValidCodePoint(codePoint);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsValidScalar(uint scalarValue)
	{
		System.Text.UnicodeUtility.IsValidUnicodeScalar(scalarValue);
	}

	[Conditional("DEBUG")]
	internal static void AssertIsValidSupplementaryPlaneScalar(uint scalarValue)
	{
		if (System.Text.UnicodeUtility.IsValidUnicodeScalar(scalarValue))
		{
			System.Text.UnicodeUtility.IsBmpCodePoint(scalarValue);
		}
	}

	private static string ToHexString(uint codePoint)
	{
		return FormattableString.Invariant($"U+{codePoint:X4}");
	}
}
