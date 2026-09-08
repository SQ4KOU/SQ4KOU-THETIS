using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIsOperator : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundExpression Operand { get; }

	public BoundTypeExpression TargetType { get; }

	public ConversionKind ConversionKind { get; }

	public BoundIsOperator(SyntaxNode syntax, BoundExpression operand, BoundTypeExpression targetType, ConversionKind conversionKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IsOperator, syntax, type, hasErrors || operand.HasErrors() || targetType.HasErrors())
	{
		Operand = operand;
		TargetType = targetType;
		ConversionKind = conversionKind;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIsOperator(this);
	}

	public BoundIsOperator Update(BoundExpression operand, BoundTypeExpression targetType, ConversionKind conversionKind, TypeSymbol type)
	{
		if (operand != Operand || targetType != TargetType || conversionKind != ConversionKind || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundIsOperator boundIsOperator = new BoundIsOperator(Syntax, operand, targetType, conversionKind, type, base.HasErrors);
			boundIsOperator.CopyAttributes(this);
			return boundIsOperator;
		}
		return this;
	}
}
