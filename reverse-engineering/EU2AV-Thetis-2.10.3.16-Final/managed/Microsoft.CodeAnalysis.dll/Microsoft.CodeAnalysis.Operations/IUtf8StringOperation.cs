namespace Microsoft.CodeAnalysis.Operations;

public interface IUtf8StringOperation : IOperation
{
	string Value { get; }
}
