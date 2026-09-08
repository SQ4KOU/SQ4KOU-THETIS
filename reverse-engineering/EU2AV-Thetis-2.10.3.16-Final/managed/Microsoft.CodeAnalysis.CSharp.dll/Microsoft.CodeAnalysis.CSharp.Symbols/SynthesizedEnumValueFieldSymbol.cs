using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEnumValueFieldSymbol : SynthesizedFieldSymbolBase
{
	internal override bool SuppressDynamicAttribute => true;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public SynthesizedEnumValueFieldSymbol(SourceNamedTypeSymbol containingEnum)
		: base(containingEnum, "value__", DeclarationModifiers.Public, isReadOnly: false, isStatic: false)
	{
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return TypeWithAnnotations.Create(((SourceNamedTypeSymbol)ContainingType).EnumUnderlyingType);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
	}
}
