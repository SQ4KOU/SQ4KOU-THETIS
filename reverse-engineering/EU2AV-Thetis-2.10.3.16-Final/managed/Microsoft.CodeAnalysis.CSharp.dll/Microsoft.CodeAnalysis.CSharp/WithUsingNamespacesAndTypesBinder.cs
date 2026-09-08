using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class WithUsingNamespacesAndTypesBinder : Binder
{
	private sealed class FromSyntax : WithUsingNamespacesAndTypesBinder
	{
		private readonly SourceNamespaceSymbol _declaringSymbol;

		private readonly CSharpSyntaxNode _declarationSyntax;

		private ImmutableArray<NamespaceOrTypeAndUsingDirective> _lazyUsings;

		internal FromSyntax(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, Binder next, bool withImportChainEntry)
			: base(next, withImportChainEntry)
		{
			_declaringSymbol = declaringSymbol;
			_declarationSyntax = declarationSyntax;
		}

		internal override ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsings(ConsList<TypeSymbol>? basesBeingResolved)
		{
			if (_lazyUsings.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyUsings, _declaringSymbol.GetUsingNamespacesOrTypes(_declarationSyntax, basesBeingResolved));
			}
			return _lazyUsings;
		}

		protected override Imports GetImports()
		{
			return _declaringSymbol.GetImports(_declarationSyntax, null);
		}
	}

	private sealed class FromSyntaxWithPreviousSubmissionImports : WithUsingNamespacesAndTypesBinder
	{
		private readonly SourceNamespaceSymbol _declaringSymbol;

		private readonly CSharpSyntaxNode _declarationSyntax;

		private Imports? _lazyFullImports;

		internal FromSyntaxWithPreviousSubmissionImports(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, Binder next, bool withImportChainEntry)
			: base(next, withImportChainEntry)
		{
			_declaringSymbol = declaringSymbol;
			_declarationSyntax = declarationSyntax;
		}

		internal override ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsings(ConsList<TypeSymbol>? basesBeingResolved)
		{
			return GetImports(basesBeingResolved).Usings;
		}

		private Imports GetImports(ConsList<TypeSymbol>? basesBeingResolved)
		{
			if (_lazyFullImports == null)
			{
				Interlocked.CompareExchange(ref _lazyFullImports, _declaringSymbol.DeclaringCompilation.GetPreviousSubmissionImports().Concat(_declaringSymbol.GetImports(_declarationSyntax, basesBeingResolved)), null);
			}
			return _lazyFullImports;
		}

		protected override Imports GetImports()
		{
			return GetImports(null);
		}
	}

	private sealed class FromNamespacesOrTypes : WithUsingNamespacesAndTypesBinder
	{
		private readonly ImmutableArray<NamespaceOrTypeAndUsingDirective> _usings;

		internal FromNamespacesOrTypes(ImmutableArray<NamespaceOrTypeAndUsingDirective> namespacesOrTypes, Binder next, bool withImportChainEntry)
			: base(next, withImportChainEntry)
		{
			_usings = namespacesOrTypes;
		}

		internal override ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsings(ConsList<TypeSymbol>? basesBeingResolved)
		{
			return _usings;
		}

		protected override Imports GetImports()
		{
			return Imports.Create(ImmutableDictionary<string, AliasAndUsingDirective>.Empty, _usings, ImmutableArray<AliasAndExternAliasDirective>.Empty);
		}
	}

	private readonly bool _withImportChainEntry;

	private ImportChain? _lazyImportChain;

	internal override bool SupportsExtensions => true;

	internal override ImportChain? ImportChain
	{
		get
		{
			if (_lazyImportChain == null)
			{
				ImportChain importChain = base.Next.ImportChain;
				if (_withImportChainEntry)
				{
					importChain = new ImportChain(GetImports(), importChain);
				}
				Interlocked.CompareExchange(ref _lazyImportChain, importChain, null);
			}
			return _lazyImportChain;
		}
	}

	protected WithUsingNamespacesAndTypesBinder(Binder next, bool withImportChainEntry)
		: base(next)
	{
		_withImportChainEntry = withImportChainEntry;
	}

	internal abstract ImmutableArray<NamespaceOrTypeAndUsingDirective> GetUsings(ConsList<TypeSymbol>? basesBeingResolved);

	protected override AssemblySymbol? GetForwardedToAssemblyInUsingNamespaces(string name, ref NamespaceOrTypeSymbol qualifierOpt, BindingDiagnosticBag diagnostics, Location location)
	{
		foreach (NamespaceOrTypeAndUsingDirective @using in GetUsings(null))
		{
			AssemblySymbol forwardedToAssembly = GetForwardedToAssembly(MetadataTypeName.FromNamespaceAndTypeName(@using.NamespaceOrType.ToString(), name), diagnostics, location);
			if (forwardedToAssembly != null)
			{
				qualifierOpt = @using.NamespaceOrType;
				return forwardedToAssembly;
			}
		}
		return base.GetForwardedToAssemblyInUsingNamespaces(name, ref qualifierOpt, diagnostics, location);
	}

	internal override void GetCandidateExtensionMethodsInSingleBinder(ArrayBuilder<MethodSymbol> methods, string name, int arity, LookupOptions options, Binder originalBinder)
	{
		bool isSemanticModelBinder = originalBinder.IsSemanticModelBinder;
		bool flag = false;
		bool flag2 = false;
		foreach (NamespaceOrTypeAndUsingDirective @using in GetUsings(null))
		{
			switch (@using.NamespaceOrType.Kind)
			{
			case SymbolKind.Namespace:
			{
				int count2 = methods.Count;
				((NamespaceSymbol)@using.NamespaceOrType).GetExtensionMethods(methods, name, arity, options);
				if (methods.Count != count2)
				{
					MarkImportDirective(@using.UsingDirectiveReference, isSemanticModelBinder);
					flag = true;
				}
				break;
			}
			case SymbolKind.NamedType:
			{
				int count = methods.Count;
				((NamedTypeSymbol)@using.NamespaceOrType).GetExtensionMethods(methods, name, arity, options);
				if (methods.Count != count)
				{
					MarkImportDirective(@using.UsingDirectiveReference, isSemanticModelBinder);
					flag2 = true;
				}
				break;
			}
			}
		}
		if (flag & flag2)
		{
			methods.RemoveDuplicates();
		}
	}

	internal override void GetCandidateExtensionMembersInSingleBinder(ArrayBuilder<Symbol> members, string? name, string? alternativeName, int arity, LookupOptions options, Binder originalBinder)
	{
		bool isSemanticModelBinder = originalBinder.IsSemanticModelBinder;
		bool flag = false;
		bool flag2 = false;
		foreach (NamespaceOrTypeAndUsingDirective @using in GetUsings(null))
		{
			if (@using.NamespaceOrType is NamespaceSymbol namespaceSymbol)
			{
				int count = members.Count;
				namespaceSymbol.GetExtensionMembers(members, name, alternativeName, arity, options, originalBinder.FieldsBeingBound);
				if (members.Count != count)
				{
					MarkImportDirective(@using.UsingDirectiveReference, isSemanticModelBinder);
					flag = true;
				}
			}
			else if (@using.NamespaceOrType is NamedTypeSymbol namedTypeSymbol)
			{
				int count2 = members.Count;
				namedTypeSymbol.GetExtensionMembers(members, name, alternativeName, arity, options, originalBinder.FieldsBeingBound);
				if (members.Count != count2)
				{
					MarkImportDirective(@using.UsingDirectiveReference, isSemanticModelBinder);
					flag2 = true;
				}
			}
		}
		if (flag & flag2)
		{
			members.RemoveDuplicates();
		}
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol>? basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool isSemanticModelBinder = originalBinder.IsSemanticModelBinder;
		foreach (NamespaceOrTypeAndUsingDirective @using in GetUsings(basesBeingResolved))
		{
			foreach (Symbol candidateMember in Binder.GetCandidateMembers(@using.NamespaceOrType, name, options, originalBinder))
			{
				if (IsValidLookupCandidateInUsings(candidateMember))
				{
					SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, arity, options, null, diagnose, ref useSiteInfo, basesBeingResolved);
					if (result2.Kind == LookupResultKind.Viable)
					{
						MarkImportDirective(@using.UsingDirectiveReference, isSemanticModelBinder);
					}
					result.MergeEqual(result2);
				}
			}
		}
	}

	private static bool IsValidLookupCandidateInUsings(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Namespace:
			return false;
		case SymbolKind.Method:
			if (!symbol.IsStatic || ((MethodSymbol)symbol).IsExtensionMethod)
			{
				return false;
			}
			break;
		default:
			if (!symbol.IsStatic)
			{
				return false;
			}
			break;
		case SymbolKind.NamedType:
			break;
		}
		return true;
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if ((options & LookupOptions.LabelsOnly) != LookupOptions.Default)
		{
			return;
		}
		options = (options & ~(LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) | LookupOptions.MustNotBeNamespace;
		foreach (NamespaceOrTypeAndUsingDirective @using in GetUsings(null))
		{
			foreach (Symbol item in @using.NamespaceOrType.GetMembersUnordered())
			{
				if (IsValidLookupCandidateInUsings(item) && originalBinder.CanAddLookupSymbolInfo(item, options, result, null))
				{
					result.AddSymbol(item, item.Name, item.GetArity());
				}
			}
		}
	}

	protected override SourceLocalSymbol? LookupLocal(SyntaxToken nameToken)
	{
		return null;
	}

	protected override LocalFunctionSymbol? LookupLocalFunction(SyntaxToken nameToken)
	{
		return null;
	}

	protected abstract Imports GetImports();

	internal static WithUsingNamespacesAndTypesBinder Create(SourceNamespaceSymbol declaringSymbol, CSharpSyntaxNode declarationSyntax, Binder next, bool withPreviousSubmissionImports = false, bool withImportChainEntry = false)
	{
		if (withPreviousSubmissionImports)
		{
			return new FromSyntaxWithPreviousSubmissionImports(declaringSymbol, declarationSyntax, next, withImportChainEntry);
		}
		return new FromSyntax(declaringSymbol, declarationSyntax, next, withImportChainEntry);
	}

	internal static WithUsingNamespacesAndTypesBinder Create(ImmutableArray<NamespaceOrTypeAndUsingDirective> namespacesOrTypes, Binder next, bool withImportChainEntry = false)
	{
		return new FromNamespacesOrTypes(namespacesOrTypes, next, withImportChainEntry);
	}
}
