using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BindingDiagnosticBag : BindingDiagnosticBag<AssemblySymbol>
{
	private static readonly ObjectPool<BindingDiagnosticBag> s_poolWithBoth = new ObjectPool<BindingDiagnosticBag>(() => new BindingDiagnosticBag(s_poolWithBoth, new DiagnosticBag(), new HashSet<AssemblySymbol>()));

	private static readonly ObjectPool<BindingDiagnosticBag> s_poolWithDiagnosticsOnly = new ObjectPool<BindingDiagnosticBag>(() => new BindingDiagnosticBag(s_poolWithDiagnosticsOnly, new DiagnosticBag(), null));

	private static readonly ObjectPool<BindingDiagnosticBag> s_poolWithDependenciesOnly = new ObjectPool<BindingDiagnosticBag>(() => new BindingDiagnosticBag(s_poolWithDependenciesOnly, null, new HashSet<AssemblySymbol>()));

	private static readonly ObjectPool<BindingDiagnosticBag> s_poolWithConcurrent = new ObjectPool<BindingDiagnosticBag>(() => new BindingDiagnosticBag(s_poolWithConcurrent, new DiagnosticBag(), new ConcurrentSet<AssemblySymbol>()));

	public static readonly BindingDiagnosticBag Discarded = new BindingDiagnosticBag(null, null);

	private readonly ObjectPool<BindingDiagnosticBag>? _pool;

	private BindingDiagnosticBag(DiagnosticBag? diagnosticBag, ICollection<AssemblySymbol>? dependenciesBag)
		: base(diagnosticBag, dependenciesBag)
	{
	}

	private BindingDiagnosticBag(ObjectPool<BindingDiagnosticBag> pool, DiagnosticBag? diagnosticBag, ICollection<AssemblySymbol>? dependenciesBag)
		: base(diagnosticBag, dependenciesBag)
	{
		_pool = pool;
	}

	internal static BindingDiagnosticBag GetInstance()
	{
		return s_poolWithBoth.Allocate();
	}

	internal static BindingDiagnosticBag GetInstance(bool withDiagnostics, bool withDependencies)
	{
		if (withDiagnostics)
		{
			if (withDependencies)
			{
				return GetInstance();
			}
			return s_poolWithDiagnosticsOnly.Allocate();
		}
		if (withDependencies)
		{
			return s_poolWithDependenciesOnly.Allocate();
		}
		return Discarded;
	}

	internal static BindingDiagnosticBag GetInstance(BindingDiagnosticBag template)
	{
		return GetInstance(template.AccumulatesDiagnostics, template.AccumulatesDependencies);
	}

	internal static BindingDiagnosticBag GetConcurrentInstance()
	{
		return s_poolWithConcurrent.Allocate();
	}

	internal override void Free()
	{
		ObjectPool<BindingDiagnosticBag> pool = _pool;
		if (pool != null)
		{
			Clear();
			pool.Free(this);
		}
		else
		{
			base.Free();
		}
	}

	internal void AddDependencies(Symbol? symbol)
	{
		if ((object)symbol != null && DependenciesBag != null)
		{
			AddDependencies(symbol.GetUseSiteInfo());
		}
	}

	internal bool ReportUseSite(Symbol? symbol, SyntaxNode node)
	{
		return ReportUseSite(symbol, (SyntaxNode syntaxNode) => syntaxNode.Location, node);
	}

	internal bool ReportUseSite(Symbol? symbol, SyntaxToken token)
	{
		return ReportUseSite(symbol, (SyntaxToken syntaxToken) => syntaxToken.GetLocation(), token);
	}

	internal bool ReportUseSite(Symbol? symbol, Location location)
	{
		return ReportUseSite(symbol, (Location result) => result, location);
	}

	internal bool ReportUseSite<TData>(Symbol? symbol, Func<TData, Location> getLocation, TData data)
	{
		if ((object)symbol != null)
		{
			return Add(symbol.GetUseSiteInfo(), getLocation, data);
		}
		return false;
	}

	internal void AddAssembliesUsedByNamespaceReference(NamespaceSymbol ns)
	{
		if (DependenciesBag != null)
		{
			addAssembliesUsedByNamespaceReferenceImpl(ns);
		}
		void addAssembliesUsedByNamespaceReferenceImpl(NamespaceSymbol namespaceSymbol)
		{
			if (namespaceSymbol.Extent.Kind == NamespaceKind.Compilation)
			{
				foreach (NamespaceSymbol constituentNamespace in namespaceSymbol.ConstituentNamespaces)
				{
					addAssembliesUsedByNamespaceReferenceImpl(constituentNamespace);
				}
			}
			else
			{
				AssemblySymbol containingAssembly = namespaceSymbol.ContainingAssembly;
				if ((object)containingAssembly != null && !containingAssembly.IsMissing)
				{
					DependenciesBag.Add(containingAssembly);
				}
			}
		}
	}

	protected override bool ReportUseSiteDiagnostic(DiagnosticInfo diagnosticInfo, DiagnosticBag diagnosticBag, Location location)
	{
		return Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnosticBag, location);
	}

	internal CSDiagnosticInfo Add(ErrorCode code, Location location)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code);
		Add(cSDiagnosticInfo, location);
		return cSDiagnosticInfo;
	}

	internal CSDiagnosticInfo Add(ErrorCode code, SyntaxNode syntax, params object[] args)
	{
		return Add(code, syntax.Location, args);
	}

	internal CSDiagnosticInfo Add(ErrorCode code, SyntaxToken syntax, params object[] args)
	{
		return Add(code, syntax.GetLocation(), args);
	}

	internal CSDiagnosticInfo Add(ErrorCode code, Location location, params object[] args)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code, args);
		Add(cSDiagnosticInfo, location);
		return cSDiagnosticInfo;
	}

	internal CSDiagnosticInfo Add(ErrorCode code, Location location, ImmutableArray<Symbol> symbols, params object[] args)
	{
		CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(code, args, symbols, ImmutableArray<Location>.Empty);
		Add(cSDiagnosticInfo, location);
		return cSDiagnosticInfo;
	}

	internal void Add(DiagnosticInfo? info, Location location)
	{
		if (info != null)
		{
			DiagnosticBag?.Add(info, location);
		}
	}
}
