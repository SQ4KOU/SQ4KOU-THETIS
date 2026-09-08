using System;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct SymbolInfo : IEquatable<SymbolInfo>
{
	internal static readonly SymbolInfo None;

	private readonly ImmutableArray<ISymbol> _candidateSymbols;

	public ISymbol? Symbol { get; }

	public ImmutableArray<ISymbol> CandidateSymbols => _candidateSymbols.NullToEmpty();

	public CandidateReason CandidateReason { get; }

	internal bool IsEmpty
	{
		get
		{
			if (Symbol == null)
			{
				return CandidateSymbols.Length == 0;
			}
			return false;
		}
	}

	internal SymbolInfo(ISymbol symbol)
		: this(symbol, ImmutableArray<ISymbol>.Empty, CandidateReason.None)
	{
	}

	internal SymbolInfo(ISymbol symbol, CandidateReason reason)
		: this(symbol, ImmutableArray<ISymbol>.Empty, reason)
	{
	}

	internal SymbolInfo(ImmutableArray<ISymbol> candidateSymbols, CandidateReason candidateReason)
		: this(null, candidateSymbols, candidateReason)
	{
	}

	private SymbolInfo(ISymbol? symbol, ImmutableArray<ISymbol> candidateSymbols, CandidateReason candidateReason)
	{
		Symbol = symbol;
		_candidateSymbols = candidateSymbols;
		CandidateReason = candidateReason;
	}

	internal ImmutableArray<ISymbol> GetAllSymbols()
	{
		if (Symbol != null)
		{
			return ImmutableArray.Create(Symbol);
		}
		return CandidateSymbols;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SymbolInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(SymbolInfo other)
	{
		if (CandidateReason == other.CandidateReason && object.Equals(Symbol, other.Symbol))
		{
			return CandidateSymbols.SequenceEqual(other.CandidateSymbols);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Symbol, Hash.Combine(Hash.CombineValues(CandidateSymbols, 4), (int)CandidateReason));
	}
}
