using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal readonly struct ReadOnlyBindingDiagnostic<TAssemblySymbol> where TAssemblySymbol : class, IAssemblySymbolInternal
{
	private readonly ImmutableArray<Diagnostic> _diagnostics;

	private readonly ImmutableArray<TAssemblySymbol> _dependencies;

	public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.NullToEmpty();

	public ImmutableArray<TAssemblySymbol> Dependencies => _dependencies.NullToEmpty();

	public static ReadOnlyBindingDiagnostic<TAssemblySymbol> Empty => new ReadOnlyBindingDiagnostic<TAssemblySymbol>(default(ImmutableArray<Diagnostic>), default(ImmutableArray<TAssemblySymbol>));

	public ReadOnlyBindingDiagnostic(ImmutableArray<Diagnostic> diagnostics, ImmutableArray<TAssemblySymbol> dependencies)
	{
		_diagnostics = diagnostics.NullToEmpty();
		_dependencies = dependencies.NullToEmpty();
	}

	public ReadOnlyBindingDiagnostic<TAssemblySymbol> NullToEmpty()
	{
		return new ReadOnlyBindingDiagnostic<TAssemblySymbol>(Diagnostics, Dependencies);
	}

	public static bool operator ==(ReadOnlyBindingDiagnostic<TAssemblySymbol> first, ReadOnlyBindingDiagnostic<TAssemblySymbol> second)
	{
		if (first.Diagnostics == second.Diagnostics)
		{
			return first.Dependencies == second.Dependencies;
		}
		return false;
	}

	public static bool operator !=(ReadOnlyBindingDiagnostic<TAssemblySymbol> first, ReadOnlyBindingDiagnostic<TAssemblySymbol> second)
	{
		return !(first == second);
	}

	public override bool Equals(object? obj)
	{
		return (obj as ReadOnlyBindingDiagnostic<TAssemblySymbol>?)?.Equals(this) ?? false;
	}

	public bool Equals(ReadOnlyBindingDiagnostic<TAssemblySymbol> other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return Diagnostics.GetHashCode();
	}

	public bool HasAnyErrors()
	{
		return Diagnostics.HasAnyErrors();
	}

	public bool HasAnyResolvedErrors()
	{
		foreach (Diagnostic diagnostic in Diagnostics)
		{
			DiagnosticWithInfo obj = diagnostic as DiagnosticWithInfo;
			if ((obj == null || !obj.HasLazyInfo) && diagnostic.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}
}
