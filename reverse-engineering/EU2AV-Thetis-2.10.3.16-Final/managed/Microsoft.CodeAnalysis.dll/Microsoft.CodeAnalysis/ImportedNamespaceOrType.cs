namespace Microsoft.CodeAnalysis;

public readonly struct ImportedNamespaceOrType
{
	public INamespaceOrTypeSymbol NamespaceOrType { get; }

	public SyntaxReference? DeclaringSyntaxReference { get; }

	internal ImportedNamespaceOrType(INamespaceOrTypeSymbol namespaceOrType, SyntaxReference? declaringSyntaxReference)
	{
		NamespaceOrType = namespaceOrType;
		DeclaringSyntaxReference = declaringSyntaxReference;
	}
}
