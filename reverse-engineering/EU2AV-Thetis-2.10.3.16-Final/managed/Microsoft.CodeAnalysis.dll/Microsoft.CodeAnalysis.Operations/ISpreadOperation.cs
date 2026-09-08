namespace Microsoft.CodeAnalysis.Operations;

public interface ISpreadOperation : IOperation
{
	IOperation Operand { get; }

	ITypeSymbol? ElementType { get; }

	CommonConversion ElementConversion { get; }
}
