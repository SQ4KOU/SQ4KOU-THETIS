using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedLambdaCacheFieldSymbol : SynthesizedFieldSymbolBase, ISynthesizedMethodBodyImplementationSymbol, ISymbolInternal
{
	private readonly TypeWithAnnotations _type;

	private readonly MethodSymbol _topLevelMethod;

	internal override bool SuppressDynamicAttribute => true;

	IMethodSymbolInternal ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => false;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public SynthesizedLambdaCacheFieldSymbol(NamedTypeSymbol containingType, TypeSymbol type, string name, MethodSymbol topLevelMethod, bool isReadOnly, bool isStatic)
		: base(containingType, name, DeclarationModifiers.Public, isReadOnly, isStatic)
	{
		_type = TypeWithAnnotations.Create(type);
		_topLevelMethod = topLevelMethod;
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
