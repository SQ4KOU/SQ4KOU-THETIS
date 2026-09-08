using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceComplexParameterSymbolWithCustomModifiersPrecedingRef : SourceComplexParameterSymbolBase
{
	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly TypeWithAnnotations _parameterType;

	public override TypeWithAnnotations TypeWithAnnotations => _parameterType;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	internal SourceComplexParameterSymbolWithCustomModifiersPrecedingRef(Symbol owner, int ordinal, TypeWithAnnotations parameterType, RefKind refKind, ImmutableArray<CustomModifier> refCustomModifiers, string name, Location location, SyntaxReference syntaxRef, bool hasParamsModifier, bool isParams, bool isExtensionMethodThis, ScopedKind scope)
		: base(owner, ordinal, refKind, name, location, syntaxRef, hasParamsModifier, isParams, isExtensionMethodThis, scope)
	{
		_parameterType = parameterType;
		_refCustomModifiers = refCustomModifiers;
	}
}
