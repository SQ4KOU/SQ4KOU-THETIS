using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class LambdaParameterSymbol : SourceComplexParameterSymbolBase
{
	private readonly TypeWithAnnotations _parameterType;

	private readonly SyntaxList<AttributeListSyntax> _attributeLists;

	public override TypeWithAnnotations TypeWithAnnotations => _parameterType;

	public override bool IsDiscard { get; }

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool IsExtensionMethodThis => false;

	public LambdaParameterSymbol(LambdaSymbol owner, SyntaxReference? syntaxRef, SyntaxList<AttributeListSyntax> attributeLists, TypeWithAnnotations parameterType, int ordinal, RefKind refKind, ScopedKind scope, string name, bool isDiscard, bool hasParamsModifier, Location location)
		: base(owner, ordinal, refKind, name, location, syntaxRef, hasParamsModifier, hasParamsModifier, isExtensionMethodThis: false, scope)
	{
		_parameterType = parameterType;
		_attributeLists = attributeLists;
		IsDiscard = isDiscard;
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(_attributeLists);
	}
}
