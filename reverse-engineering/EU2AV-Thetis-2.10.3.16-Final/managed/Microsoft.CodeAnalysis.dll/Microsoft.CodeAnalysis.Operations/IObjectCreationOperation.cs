using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IObjectCreationOperation : IOperation
{
	IMethodSymbol? Constructor { get; }

	IObjectOrCollectionInitializerOperation? Initializer { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }
}
