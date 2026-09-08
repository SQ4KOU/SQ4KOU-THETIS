namespace Microsoft.CodeAnalysis.Operations;

public interface IInterpolatedStringAppendOperation : IInterpolatedStringContentOperation, IOperation
{
	IOperation AppendCall { get; }
}
