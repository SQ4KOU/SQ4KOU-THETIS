namespace Microsoft.CodeAnalysis.Operations;

public interface IAttributeOperation : IOperation
{
	IOperation Operation { get; }
}
