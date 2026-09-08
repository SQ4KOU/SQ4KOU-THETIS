using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class DiagnosticAnalyzer
{
	public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

	public abstract void Initialize(AnalysisContext context);

	public sealed override bool Equals(object? obj)
	{
		return this == obj;
	}

	public sealed override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public sealed override string ToString()
	{
		return GetType().ToString();
	}
}
