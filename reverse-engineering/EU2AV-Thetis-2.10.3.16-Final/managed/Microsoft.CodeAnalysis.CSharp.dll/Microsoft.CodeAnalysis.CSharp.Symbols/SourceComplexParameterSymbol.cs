using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceComplexParameterSymbol : SourceComplexParameterSymbolBase
{
	private readonly TypeWithAnnotations _parameterType;

	public override TypeWithAnnotations TypeWithAnnotations => _parameterType;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal SourceComplexParameterSymbol(Symbol owner, int ordinal, TypeWithAnnotations parameterType, RefKind refKind, string name, Location location, SyntaxReference syntaxRef, bool hasParamsModifier, bool isParams, bool isExtensionMethodThis, ScopedKind scope)
		: base(owner, ordinal, refKind, name, location, syntaxRef, hasParamsModifier, isParams, isExtensionMethodThis, scope)
	{
		_parameterType = parameterType;
	}
}
