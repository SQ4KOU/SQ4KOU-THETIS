using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithMethodTypeParametersBinder : WithTypeParametersBinder
{
	internal const LookupOptions MethodTypeParameterLookupMask = LookupOptions.NamespaceAliasesOnly | LookupOptions.MustNotBeMethodTypeParameter;

	private readonly MethodSymbol _methodSymbol;

	private MultiDictionary<string, TypeParameterSymbol> _lazyTypeParameterMap;

	protected override bool InExecutableBinder => false;

	internal override Symbol ContainingMemberOrLambda => _methodSymbol;

	protected override MultiDictionary<string, TypeParameterSymbol> TypeParameterMap
	{
		get
		{
			if (_lazyTypeParameterMap == null)
			{
				MultiDictionary<string, TypeParameterSymbol> multiDictionary = new MultiDictionary<string, TypeParameterSymbol>();
				foreach (TypeParameterSymbol typeParameter in _methodSymbol.TypeParameters)
				{
					multiDictionary.Add(typeParameter.Name, typeParameter);
				}
				Interlocked.CompareExchange(ref _lazyTypeParameterMap, multiDictionary, null);
			}
			return _lazyTypeParameterMap;
		}
	}

	protected override LookupOptions LookupMask => LookupOptions.NamespaceAliasesOnly | LookupOptions.MustNotBeMethodTypeParameter;

	internal WithMethodTypeParametersBinder(MethodSymbol methodSymbol, Binder next)
		: base(next)
	{
		_methodSymbol = methodSymbol;
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!CanConsiderTypeParameters(options))
		{
			return;
		}
		foreach (TypeParameterSymbol typeParameter in _methodSymbol.TypeParameters)
		{
			if (originalBinder.CanAddLookupSymbolInfo(typeParameter, options, result, null))
			{
				result.AddSymbol(typeParameter, typeParameter.Name, 0);
			}
		}
	}
}
