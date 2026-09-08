using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SameDiagnosticComparer : EqualityComparer<Diagnostic>
{
	public static readonly SameDiagnosticComparer Instance = new SameDiagnosticComparer();

	public override bool Equals(Diagnostic? x, Diagnostic? y)
	{
		return x?.Equals(y) ?? (y == null);
	}

	public override int GetHashCode(Diagnostic obj)
	{
		return obj.GetHashCode();
	}
}
