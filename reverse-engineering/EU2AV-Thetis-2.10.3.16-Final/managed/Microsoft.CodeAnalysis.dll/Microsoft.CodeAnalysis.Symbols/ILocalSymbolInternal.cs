namespace Microsoft.CodeAnalysis.Symbols;

internal interface ILocalSymbolInternal : ISymbolInternal
{
	bool IsImportedFromMetadata { get; }

	SynthesizedLocalKind SynthesizedKind { get; }

	SyntaxNode GetDeclaratorSyntax();
}
