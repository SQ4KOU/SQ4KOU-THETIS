namespace Microsoft.CodeAnalysis.Symbols;

internal interface ITypeSymbolInternal : INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	TypeKind TypeKind { get; }

	SpecialType SpecialType { get; }

	ExtendedSpecialType ExtendedSpecialType { get; }

	bool IsReferenceType { get; }

	bool IsValueType { get; }

	ITypeSymbol GetITypeSymbol();
}
