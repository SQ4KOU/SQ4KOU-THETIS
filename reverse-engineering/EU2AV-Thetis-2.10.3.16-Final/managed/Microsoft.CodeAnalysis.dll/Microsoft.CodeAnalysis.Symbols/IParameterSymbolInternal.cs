namespace Microsoft.CodeAnalysis.Symbols;

internal interface IParameterSymbolInternal : ISymbolInternal
{
	ITypeSymbolInternal Type { get; }

	RefKind RefKind { get; }
}
