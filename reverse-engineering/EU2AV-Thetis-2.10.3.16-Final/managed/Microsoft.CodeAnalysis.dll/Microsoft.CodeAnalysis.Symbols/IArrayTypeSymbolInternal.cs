namespace Microsoft.CodeAnalysis.Symbols;

internal interface IArrayTypeSymbolInternal : ITypeSymbolInternal, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	bool IsSZArray { get; }

	ITypeSymbolInternal ElementType { get; }
}
