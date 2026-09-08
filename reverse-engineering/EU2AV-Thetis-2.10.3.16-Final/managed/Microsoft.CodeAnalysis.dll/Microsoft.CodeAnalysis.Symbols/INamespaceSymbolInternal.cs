namespace Microsoft.CodeAnalysis.Symbols;

internal interface INamespaceSymbolInternal : INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	bool IsGlobalNamespace { get; }
}
