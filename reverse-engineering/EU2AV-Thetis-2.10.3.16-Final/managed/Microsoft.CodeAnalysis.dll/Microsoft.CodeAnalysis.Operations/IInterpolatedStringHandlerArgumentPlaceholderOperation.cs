namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringHandlerArgumentPlaceholderOperation : IOperation
{
	int ArgumentIndex { get; }

	InterpolatedStringArgumentPlaceholderKind PlaceholderKind { get; }
}
