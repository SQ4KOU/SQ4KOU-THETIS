namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringAdditionOperation : IOperation
{
	IOperation Left { get; }

	IOperation Right { get; }
}
