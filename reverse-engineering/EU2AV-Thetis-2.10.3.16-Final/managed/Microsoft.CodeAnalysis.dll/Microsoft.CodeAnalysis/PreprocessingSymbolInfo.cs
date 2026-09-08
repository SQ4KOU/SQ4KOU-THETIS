using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public readonly struct PreprocessingSymbolInfo : IEquatable<PreprocessingSymbolInfo>
{
	internal static readonly PreprocessingSymbolInfo None = new PreprocessingSymbolInfo(null, isDefined: false);

	public IPreprocessingSymbol? Symbol { get; }

	public bool IsDefined { get; }

	internal PreprocessingSymbolInfo(IPreprocessingSymbol? symbol, bool isDefined)
	{
		this = default(PreprocessingSymbolInfo);
		Symbol = symbol;
		IsDefined = isDefined;
	}

	public bool Equals(PreprocessingSymbolInfo other)
	{
		if (object.Equals(Symbol, other.Symbol))
		{
			return object.Equals(IsDefined, other.IsDefined);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is PreprocessingSymbolInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(IsDefined, Hash.Combine(Symbol, 0));
	}
}
