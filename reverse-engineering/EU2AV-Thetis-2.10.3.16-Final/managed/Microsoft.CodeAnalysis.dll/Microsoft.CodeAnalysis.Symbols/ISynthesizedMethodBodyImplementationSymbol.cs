namespace Microsoft.CodeAnalysis.Symbols;

internal interface ISynthesizedMethodBodyImplementationSymbol : ISymbolInternal
{
	IMethodSymbolInternal? Method { get; }

	bool HasMethodBodyDependency { get; }
}
