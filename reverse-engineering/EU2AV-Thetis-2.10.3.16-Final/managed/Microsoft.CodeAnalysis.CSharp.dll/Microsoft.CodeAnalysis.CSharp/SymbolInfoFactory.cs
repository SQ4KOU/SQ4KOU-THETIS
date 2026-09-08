using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class SymbolInfoFactory
{
	internal static SymbolInfo Create(ImmutableArray<Symbol> symbols, LookupResultKind resultKind, bool isDynamic)
	{
		return Create(OneOrMany.Create(symbols.NullToEmpty()), resultKind, isDynamic);
	}

	internal static SymbolInfo Create(OneOrMany<Symbol> symbols, LookupResultKind resultKind, bool isDynamic)
	{
		if (isDynamic)
		{
			if (symbols.Count == 1)
			{
				return new SymbolInfo(symbols[0].GetPublicSymbol(), CandidateReason.LateBound);
			}
			return new SymbolInfo(getPublicSymbols(symbols), CandidateReason.LateBound);
		}
		if (resultKind == LookupResultKind.Viable)
		{
			if (symbols.Count > 0)
			{
				return new SymbolInfo(symbols[0].GetPublicSymbol());
			}
			return SymbolInfo.None;
		}
		return new SymbolInfo(getPublicSymbols(symbols), (symbols.Count > 0) ? resultKind.ToCandidateReason() : CandidateReason.None);
		static ImmutableArray<ISymbol> getPublicSymbols(OneOrMany<Symbol> oneOrMany)
		{
			ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance(oneOrMany.Count);
			foreach (Symbol item in oneOrMany)
			{
				instance.Add(item.GetPublicSymbol());
			}
			return instance.ToImmutableAndFree();
		}
	}
}
