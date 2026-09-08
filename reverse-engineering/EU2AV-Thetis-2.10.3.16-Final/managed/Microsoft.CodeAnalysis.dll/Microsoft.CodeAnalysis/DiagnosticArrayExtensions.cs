using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal static class DiagnosticArrayExtensions
{
	internal static bool HasAnyErrors<T>(this ImmutableArray<T> diagnostics) where T : Diagnostic
	{
		foreach (T item in diagnostics)
		{
			if (item.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}
}
