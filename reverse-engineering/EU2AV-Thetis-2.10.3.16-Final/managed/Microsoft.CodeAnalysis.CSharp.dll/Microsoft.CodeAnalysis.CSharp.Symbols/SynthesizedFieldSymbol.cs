using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedFieldSymbol : SynthesizedFieldSymbolBase
{
	private readonly TypeWithAnnotations _type;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool SuppressDynamicAttribute => true;

	public SynthesizedFieldSymbol(NamedTypeSymbol containingType, TypeSymbol type, string name, DeclarationModifiers accessibility = DeclarationModifiers.Private, bool isReadOnly = false, bool isStatic = false)
		: base(containingType, name, accessibility, isReadOnly, isStatic)
	{
		_type = TypeWithAnnotations.Create(type);
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
