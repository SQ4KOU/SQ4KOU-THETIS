using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.Cci;

internal interface INamespace : INamedEntity
{
	INamespace ContainingNamespace { get; }

	INamespaceSymbolInternal GetInternalSymbol();
}
