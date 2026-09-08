using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IInvocationOperation : IOperation
{
	IMethodSymbol TargetMethod { get; }

	ITypeSymbol? ConstrainedToType { get; }

	IOperation? Instance { get; }

	bool IsVirtual { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }
}
