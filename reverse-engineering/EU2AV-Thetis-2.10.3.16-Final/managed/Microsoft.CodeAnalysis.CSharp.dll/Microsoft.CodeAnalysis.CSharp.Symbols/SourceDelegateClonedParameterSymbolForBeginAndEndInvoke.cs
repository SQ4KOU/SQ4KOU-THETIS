using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceDelegateClonedParameterSymbolForBeginAndEndInvoke : SourceClonedParameterSymbol
{
	internal override bool IsCallerFilePath => _originalParam.IsCallerFilePath;

	internal override bool IsCallerLineNumber => _originalParam.IsCallerLineNumber;

	internal override bool IsCallerMemberName => _originalParam.IsCallerMemberName;

	internal override int CallerArgumentExpressionParameterIndex => -1;

	internal SourceDelegateClonedParameterSymbolForBeginAndEndInvoke(SourceParameterSymbol originalParam, SourceDelegateMethodSymbol newOwner, int newOrdinal)
		: base(originalParam, newOwner, newOrdinal, suppressOptional: true)
	{
	}

	internal override ParameterSymbol WithCustomModifiersAndParams(TypeSymbol newType, ImmutableArray<CustomModifier> newCustomModifiers, ImmutableArray<CustomModifier> newRefCustomModifiers, bool newIsParams)
	{
		return new SourceDelegateClonedParameterSymbolForBeginAndEndInvoke(_originalParam.WithCustomModifiersAndParamsCore(newType, newCustomModifiers, newRefCustomModifiers, newIsParams), (SourceDelegateMethodSymbol)ContainingSymbol, Ordinal);
	}
}
