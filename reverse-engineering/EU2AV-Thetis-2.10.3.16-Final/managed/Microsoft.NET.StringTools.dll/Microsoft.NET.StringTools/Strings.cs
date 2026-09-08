using System;

namespace Microsoft.NET.StringTools;

public static class Strings
{
	[ThreadStatic]
	private static SpanBasedStringBuilder? _spanBasedStringBuilder;

	public static string WeakIntern(string str)
	{
		InternableString candidate = new InternableString(str);
		return WeakStringCacheInterner.Instance.InternableToString(ref candidate);
	}

	public static string WeakIntern(ReadOnlySpan<char> str)
	{
		InternableString candidate = new InternableString(str);
		return WeakStringCacheInterner.Instance.InternableToString(ref candidate);
	}

	public static SpanBasedStringBuilder GetSpanBasedStringBuilder()
	{
		SpanBasedStringBuilder spanBasedStringBuilder = _spanBasedStringBuilder;
		if (spanBasedStringBuilder == null)
		{
			return new SpanBasedStringBuilder();
		}
		_spanBasedStringBuilder = null;
		return spanBasedStringBuilder;
	}

	public static void EnableDiagnostics()
	{
		WeakStringCacheInterner.Instance.EnableStatistics();
	}

	public static string CreateDiagnosticReport()
	{
		return WeakStringCacheInterner.Instance.FormatStatistics();
	}

	public static void ClearCachedStrings()
	{
		WeakStringCacheInterner.Instance.Dispose();
	}

	internal static void ReturnSpanBasedStringBuilder(SpanBasedStringBuilder stringBuilder)
	{
		stringBuilder.Clear();
		_spanBasedStringBuilder = stringBuilder;
	}
}
