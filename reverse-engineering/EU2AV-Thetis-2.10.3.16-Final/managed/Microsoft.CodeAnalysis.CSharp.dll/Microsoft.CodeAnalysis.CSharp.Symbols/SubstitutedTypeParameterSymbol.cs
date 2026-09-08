using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedTypeParameterSymbol : SubstitutedTypeParameterSymbolBase
{
	public override TypeParameterSymbol OriginalDefinition => _underlyingTypeParameter.OriginalDefinition;

	internal SubstitutedTypeParameterSymbol(Symbol newContainer, TypeMap map, TypeParameterSymbol substitutedFrom, int ordinal)
		: base(newContainer, map, substitutedFrom, ordinal)
	{
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
	}
}
