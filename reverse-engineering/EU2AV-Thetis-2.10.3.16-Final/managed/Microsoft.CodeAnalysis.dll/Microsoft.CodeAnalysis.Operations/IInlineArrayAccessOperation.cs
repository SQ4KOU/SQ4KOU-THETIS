namespace Microsoft.CodeAnalysis.Operations;

public interface IInlineArrayAccessOperation : IOperation
{
	IOperation Instance { get; }

	IOperation Argument { get; }
}
