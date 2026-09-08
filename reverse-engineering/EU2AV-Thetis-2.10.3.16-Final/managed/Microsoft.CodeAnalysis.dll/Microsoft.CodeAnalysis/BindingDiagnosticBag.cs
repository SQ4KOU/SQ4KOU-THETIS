using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal abstract class BindingDiagnosticBag
{
	public readonly DiagnosticBag? DiagnosticBag;

	[MemberNotNullWhen(true, "DiagnosticBag")]
	internal bool AccumulatesDiagnostics
	{
		[MemberNotNullWhen(true, "DiagnosticBag")]
		get
		{
			return DiagnosticBag != null;
		}
	}

	protected BindingDiagnosticBag(DiagnosticBag? diagnosticBag)
	{
		DiagnosticBag = diagnosticBag;
	}

	internal void AddRange<T>(ImmutableArray<T> diagnostics) where T : Diagnostic
	{
		DiagnosticBag?.AddRange(diagnostics);
	}

	internal void AddRange(IEnumerable<Diagnostic> diagnostics)
	{
		DiagnosticBag?.AddRange(diagnostics);
	}

	internal bool HasAnyResolvedErrors()
	{
		return DiagnosticBag?.HasAnyResolvedErrors() ?? false;
	}

	internal bool HasAnyErrors()
	{
		return DiagnosticBag?.HasAnyErrors() ?? false;
	}

	internal void Add(Diagnostic diag)
	{
		DiagnosticBag?.Add(diag);
	}
}
internal abstract class BindingDiagnosticBag<TAssemblySymbol> : BindingDiagnosticBag where TAssemblySymbol : class, IAssemblySymbolInternal
{
	public readonly ICollection<TAssemblySymbol>? DependenciesBag;

	internal bool AccumulatesDependencies => DependenciesBag != null;

	protected BindingDiagnosticBag(DiagnosticBag? diagnosticBag, ICollection<TAssemblySymbol>? dependenciesBag)
		: base(diagnosticBag)
	{
		DependenciesBag = dependenciesBag;
	}

	protected BindingDiagnosticBag(bool usePool)
		: this(usePool ? Microsoft.CodeAnalysis.DiagnosticBag.GetInstance() : new DiagnosticBag(), (ICollection<TAssemblySymbol>?)(usePool ? PooledHashSet<TAssemblySymbol>.GetInstance() : new HashSet<TAssemblySymbol>()))
	{
	}

	internal virtual void Free()
	{
		DiagnosticBag?.Free();
		((PooledHashSet<TAssemblySymbol>)DependenciesBag)?.Free();
	}

	internal ReadOnlyBindingDiagnostic<TAssemblySymbol> ToReadOnly(bool forceDiagnosticResolution = true)
	{
		return new ReadOnlyBindingDiagnostic<TAssemblySymbol>(DiagnosticBag?.ToReadOnly(forceDiagnosticResolution) ?? default(ImmutableArray<Diagnostic>), DependenciesBag?.ToImmutableArray() ?? default(ImmutableArray<TAssemblySymbol>));
	}

	internal ReadOnlyBindingDiagnostic<TAssemblySymbol> ToReadOnlyAndFree(bool forceDiagnosticResolution = true)
	{
		ReadOnlyBindingDiagnostic<TAssemblySymbol> result = ToReadOnly(forceDiagnosticResolution);
		Free();
		return result;
	}

	internal void AddRangeAndFree(BindingDiagnosticBag<TAssemblySymbol> other)
	{
		AddRange(other);
		other.Free();
	}

	internal void Clear()
	{
		DiagnosticBag?.Clear();
		DependenciesBag?.Clear();
	}

	internal void AddRange(ReadOnlyBindingDiagnostic<TAssemblySymbol> other, bool allowMismatchInDependencyAccumulation = false)
	{
		AddRange(other.Diagnostics);
		AddDependencies(other.Dependencies);
	}

	internal void AddRange(BindingDiagnosticBag<TAssemblySymbol>? other, bool allowMismatchInDependencyAccumulation = false)
	{
		if (other != null)
		{
			AddRange(other.DiagnosticBag);
			AddDependencies(other.DependenciesBag);
		}
	}

	internal void AddRange(DiagnosticBag? bag)
	{
		if (bag != null)
		{
			DiagnosticBag?.AddRange(bag);
		}
	}

	internal void AddDependency(TAssemblySymbol? dependency)
	{
		if (dependency != null && DependenciesBag != null)
		{
			DependenciesBag.Add(dependency);
		}
	}

	internal void AddDependencies(ICollection<TAssemblySymbol>? dependencies)
	{
		if (dependencies.IsNullOrEmpty() || DependenciesBag == null)
		{
			return;
		}
		foreach (TAssemblySymbol dependency in dependencies)
		{
			DependenciesBag.Add(dependency);
		}
	}

	internal void AddDependencies(IReadOnlyCollection<TAssemblySymbol>? dependencies)
	{
		if (dependencies.IsNullOrEmpty() || DependenciesBag == null)
		{
			return;
		}
		foreach (TAssemblySymbol dependency in dependencies)
		{
			DependenciesBag.Add(dependency);
		}
	}

	internal void AddDependencies(ImmutableHashSet<TAssemblySymbol>? dependencies)
	{
		if (dependencies.IsNullOrEmpty() || DependenciesBag == null)
		{
			return;
		}
		foreach (TAssemblySymbol dependency in dependencies)
		{
			DependenciesBag.Add(dependency);
		}
	}

	internal void AddDependencies(ImmutableArray<TAssemblySymbol> dependencies)
	{
		if (!dependencies.IsDefaultOrEmpty && DependenciesBag != null)
		{
			foreach (TAssemblySymbol item in dependencies)
			{
				DependenciesBag.Add(item);
			}
		}
	}

	internal void AddDependencies(BindingDiagnosticBag<TAssemblySymbol> dependencies, bool allowMismatchInDependencyAccumulation = false)
	{
		AddDependencies(dependencies.DependenciesBag);
	}

	internal void AddDependencies(UseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		if (DependenciesBag != null)
		{
			AddDependency(useSiteInfo.PrimaryDependency);
			AddDependencies(useSiteInfo.SecondaryDependencies);
		}
	}

	internal void AddDependencies(CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		if (DependenciesBag != null)
		{
			AddDependencies(useSiteInfo.Dependencies);
		}
	}

	internal bool Add(SyntaxNode node, CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		return Add(useSiteInfo, (SyntaxNode syntaxNode) => syntaxNode.Location, node);
	}

	internal bool AddDiagnostics(SyntaxNode node, CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		return AddDiagnostics(useSiteInfo, (SyntaxNode syntaxNode) => syntaxNode.Location, node);
	}

	internal bool Add(SyntaxToken token, CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		return Add(useSiteInfo, (SyntaxToken syntaxToken) => syntaxToken.GetLocation(), token);
	}

	internal bool Add(Location location, CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		return Add(useSiteInfo, (Location result) => result, location);
	}

	internal bool AddDiagnostics(Location location, CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo)
	{
		return AddDiagnostics(useSiteInfo, (Location result) => result, location);
	}

	internal bool AddDiagnostics<TData>(CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo, Func<TData, Location> getLocation, TData data)
	{
		DiagnosticBag diagnosticBag = DiagnosticBag;
		if (diagnosticBag != null)
		{
			if (!useSiteInfo.Diagnostics.IsNullOrEmpty())
			{
				bool flag = false;
				Location location = getLocation(data);
				foreach (DiagnosticInfo diagnostic in useSiteInfo.Diagnostics)
				{
					if (ReportUseSiteDiagnostic(diagnostic, diagnosticBag, location))
					{
						flag = true;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		else if (useSiteInfo.AccumulatesDiagnostics && !useSiteInfo.Diagnostics.IsNullOrEmpty())
		{
			foreach (DiagnosticInfo diagnostic2 in useSiteInfo.Diagnostics)
			{
				if (diagnostic2.Severity == DiagnosticSeverity.Error)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool Add<TData>(CompoundUseSiteInfo<TAssemblySymbol> useSiteInfo, Func<TData, Location> getLocation, TData data)
	{
		if (AddDiagnostics(useSiteInfo, getLocation, data))
		{
			return true;
		}
		AddDependencies(useSiteInfo);
		return false;
	}

	protected abstract bool ReportUseSiteDiagnostic(DiagnosticInfo diagnosticInfo, DiagnosticBag diagnosticBag, Location location);

	internal bool Add(UseSiteInfo<TAssemblySymbol> useSiteInfo, SyntaxNode node)
	{
		return Add(useSiteInfo, (SyntaxNode syntaxNode) => syntaxNode.Location, node);
	}

	internal bool Add(UseSiteInfo<TAssemblySymbol> useSiteInfo, Location location)
	{
		return Add(useSiteInfo, (Location result) => result, location);
	}

	internal bool Add(UseSiteInfo<TAssemblySymbol> useSiteInfo, SyntaxToken token)
	{
		return Add(useSiteInfo, (SyntaxToken syntaxToken) => syntaxToken.GetLocation(), token);
	}

	internal bool Add<TData>(UseSiteInfo<TAssemblySymbol> info, Func<TData, Location> getLocation, TData data)
	{
		if (ReportUseSiteDiagnostic(info.DiagnosticInfo, getLocation, data))
		{
			return true;
		}
		AddDependencies(info);
		return false;
	}

	internal bool ReportUseSiteDiagnostic(DiagnosticInfo? info, Location location)
	{
		return ReportUseSiteDiagnostic(info, (Location result) => result, location);
	}

	internal bool ReportUseSiteDiagnostic<TData>(DiagnosticInfo? info, Func<TData, Location> getLocation, TData data)
	{
		if (info == null)
		{
			return false;
		}
		if (DiagnosticBag != null)
		{
			return ReportUseSiteDiagnostic(info, DiagnosticBag, getLocation(data));
		}
		return info.Severity == DiagnosticSeverity.Error;
	}
}
