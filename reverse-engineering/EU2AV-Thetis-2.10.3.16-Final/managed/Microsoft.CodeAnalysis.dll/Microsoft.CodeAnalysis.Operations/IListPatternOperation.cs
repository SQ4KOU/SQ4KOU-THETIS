using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IListPatternOperation : IPatternOperation, IOperation
{
	ISymbol? LengthSymbol { get; }

	ISymbol? IndexerSymbol { get; }

	ImmutableArray<IPatternOperation> Patterns { get; }

	ISymbol? DeclaredSymbol { get; }
}
