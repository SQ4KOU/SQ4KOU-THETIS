using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNullCoalescingOperator : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundExpression LeftOperand { get; }

	public BoundExpression RightOperand { get; }

	public BoundValuePlaceholder? LeftPlaceholder { get; }

	public BoundExpression? LeftConversion { get; }

	public BoundNullCoalescingOperatorResultKind OperatorResultKind { get; }

	public bool Checked { get; }

	public BoundNullCoalescingOperator(SyntaxNode syntax, BoundExpression leftOperand, BoundExpression rightOperand, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundNullCoalescingOperatorResultKind operatorResultKind, bool @checked, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NullCoalescingOperator, syntax, type, hasErrors || leftOperand.HasErrors() || rightOperand.HasErrors() || leftPlaceholder.HasErrors() || leftConversion.HasErrors())
	{
		LeftOperand = leftOperand;
		RightOperand = rightOperand;
		LeftPlaceholder = leftPlaceholder;
		LeftConversion = leftConversion;
		OperatorResultKind = operatorResultKind;
		Checked = @checked;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNullCoalescingOperator(this);
	}

	public BoundNullCoalescingOperator Update(BoundExpression leftOperand, BoundExpression rightOperand, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundNullCoalescingOperatorResultKind operatorResultKind, bool @checked, TypeSymbol type)
	{
		if (leftOperand != LeftOperand || rightOperand != RightOperand || leftPlaceholder != LeftPlaceholder || leftConversion != LeftConversion || operatorResultKind != OperatorResultKind || @checked != Checked || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundNullCoalescingOperator boundNullCoalescingOperator = new BoundNullCoalescingOperator(Syntax, leftOperand, rightOperand, leftPlaceholder, leftConversion, operatorResultKind, @checked, type, base.HasErrors);
			boundNullCoalescingOperator.CopyAttributes(this);
			return boundNullCoalescingOperator;
		}
		return this;
	}
}
