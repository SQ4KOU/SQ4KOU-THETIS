using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface ICollectionExpressionOperation : IOperation
{
	IMethodSymbol? ConstructMethod { get; }

	ImmutableArray<IOperation> Elements { get; }
}
