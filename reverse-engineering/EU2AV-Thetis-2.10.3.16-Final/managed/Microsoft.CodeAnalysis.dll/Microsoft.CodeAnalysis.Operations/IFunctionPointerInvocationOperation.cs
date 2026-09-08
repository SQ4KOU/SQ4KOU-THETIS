using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

public interface IFunctionPointerInvocationOperation : IOperation
{
	IOperation Target { get; }

	ImmutableArray<IArgumentOperation> Arguments { get; }
}
