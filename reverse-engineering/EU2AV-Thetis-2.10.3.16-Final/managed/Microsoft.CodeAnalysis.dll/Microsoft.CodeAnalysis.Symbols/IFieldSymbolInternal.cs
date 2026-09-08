namespace Microsoft.CodeAnalysis.Symbols;

internal interface IFieldSymbolInternal : ISymbolInternal
{
	ISymbolInternal? AssociatedSymbol { get; }

	bool IsVolatile { get; }

	ITypeSymbolInternal Type { get; }
}
