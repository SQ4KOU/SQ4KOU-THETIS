namespace Microsoft.CodeAnalysis.Operations;

public interface IImplicitIndexerReferenceOperation : IOperation
{
	IOperation Instance { get; }

	IOperation Argument { get; }

	ISymbol LengthSymbol { get; }

	ISymbol IndexerSymbol { get; }
}
