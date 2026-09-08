using System.Diagnostics.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class RoslynString
{
	public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
	{
		return string.IsNullOrEmpty(value);
	}

	public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
	{
		return string.IsNullOrWhiteSpace(value);
	}
}
