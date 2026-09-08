using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAsOperator : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundExpression Operand { get; }

	public BoundTypeExpression TargetType { get; }

	public BoundValuePlaceholder? OperandPlaceholder { get; }

	public BoundExpression? OperandConversion { get; }

	public BoundAsOperator(SyntaxNode syntax, BoundExpression operand, BoundTypeExpression targetType, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AsOperator, syntax, type, hasErrors || operand.HasErrors() || targetType.HasErrors() || operandPlaceholder.HasErrors() || operandConversion.HasErrors())
	{
		Operand = operand;
		TargetType = targetType;
		OperandPlaceholder = operandPlaceholder;
		OperandConversion = operandConversion;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAsOperator(this);
	}

	public BoundAsOperator Update(BoundExpression operand, BoundTypeExpression targetType, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, TypeSymbol type)
	{
		if (operand != Operand || targetType != TargetType || operandPlaceholder != OperandPlaceholder || operandConversion != OperandConversion || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundAsOperator boundAsOperator = new BoundAsOperator(Syntax, operand, targetType, operandPlaceholder, operandConversion, type, base.HasErrors);
			boundAsOperator.CopyAttributes(this);
			return boundAsOperator;
		}
		return this;
	}
}
