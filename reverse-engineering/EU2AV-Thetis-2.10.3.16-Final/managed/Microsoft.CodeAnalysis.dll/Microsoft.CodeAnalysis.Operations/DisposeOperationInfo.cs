using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal readonly struct DisposeOperationInfo(IMethodSymbol? disposeMethod, ImmutableArray<IArgumentOperation> disposeArguments)
{
	public readonly IMethodSymbol? DisposeMethod = disposeMethod;

	public readonly ImmutableArray<IArgumentOperation> DisposeArguments = disposeArguments;
}
