using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithPrimaryConstructorParametersBinder : Binder
{
	private readonly NamedTypeSymbol _type;

	private MethodSymbol? _lazyPrimaryCtorWithParameters = ErrorMethodSymbol.UnknownMethod;

	private MultiDictionary<string, ParameterSymbol>? _lazyParameterMap;

	internal WithPrimaryConstructorParametersBinder(NamedTypeSymbol type, Binder next)
		: base(next)
	{
		_type = type;
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!options.CanConsiderMembers())
		{
			return;
		}
		EnsurePrimaryConstructor();
		if ((object)_lazyPrimaryCtorWithParameters == null)
		{
			return;
		}
		foreach (ParameterSymbol parameter in _lazyPrimaryCtorWithParameters.Parameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(parameter, options, result, null))
			{
				result.AddSymbol(parameter, parameter.Name, 0);
			}
		}
	}

	private void EnsurePrimaryConstructor()
	{
		if ((object)_lazyPrimaryCtorWithParameters != ErrorMethodSymbol.UnknownMethod)
		{
			return;
		}
		if (_type is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol)
		{
			SynthesizedPrimaryConstructor primaryConstructor = sourceMemberContainerTypeSymbol.PrimaryConstructor;
			if ((object)primaryConstructor != null && primaryConstructor.ParameterCount != 0)
			{
				_lazyPrimaryCtorWithParameters = primaryConstructor;
				return;
			}
		}
		_lazyPrimaryCtorWithParameters = null;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) != LookupOptions.Default)
		{
			return;
		}
		EnsurePrimaryConstructor();
		if ((object)_lazyPrimaryCtorWithParameters == null)
		{
			return;
		}
		MultiDictionary<string, ParameterSymbol> multiDictionary = _lazyParameterMap;
		if (multiDictionary == null)
		{
			ImmutableArray<ParameterSymbol> parameters = _lazyPrimaryCtorWithParameters.Parameters;
			multiDictionary = new MultiDictionary<string, ParameterSymbol>(parameters.Length, EqualityComparer<string>.Default);
			foreach (ParameterSymbol item in parameters)
			{
				multiDictionary.Add(item.Name, item);
			}
			_lazyParameterMap = multiDictionary;
		}
		foreach (ParameterSymbol item2 in multiDictionary[name])
		{
			result.MergeEqual(originalBinder.CheckViability(item2, arity, options, null, diagnose, ref useSiteInfo));
		}
	}
}
