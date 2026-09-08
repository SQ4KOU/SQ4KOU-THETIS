using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUnaryOperator : BoundExpression
{
	public override Symbol? ExpressionSymbol => MethodOpt;

	public new TypeSymbol Type => base.Type;

	public UnaryOperatorKind OperatorKind { get; }

	public BoundExpression Operand { get; }

	public override ConstantValue? ConstantValueOpt { get; }

	public MethodSymbol? MethodOpt { get; }

	public TypeSymbol? ConstrainedToTypeOpt { get; }

	public override LookupResultKind ResultKind { get; }

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BoundUnaryOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, operand, constantValueOpt, methodOpt, constrainedToTypeOpt, resultKind, default(ImmutableArray<MethodSymbol>), type, hasErrors)
	{
	}

	public BoundUnaryOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, TypeSymbol type)
	{
		return Update(operatorKind, operand, constantValueOpt, methodOpt, constrainedToTypeOpt, resultKind, OriginalUserDefinedOperatorsOpt, type);
	}

	public BoundUnaryOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.UnaryOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		OperatorKind = operatorKind;
		Operand = operand;
		ConstantValueOpt = constantValueOpt;
		MethodOpt = methodOpt;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
		ResultKind = resultKind;
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnaryOperator(this);
	}

	public BoundUnaryOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || operand != Operand || constantValueOpt != ConstantValueOpt || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(methodOpt, MethodOpt) || !TypeSymbol.Equals(constrainedToTypeOpt, ConstrainedToTypeOpt, TypeCompareKind.ConsiderEverything) || resultKind != ResultKind || originalUserDefinedOperatorsOpt != OriginalUserDefinedOperatorsOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundUnaryOperator boundUnaryOperator = new BoundUnaryOperator(Syntax, operatorKind, operand, constantValueOpt, methodOpt, constrainedToTypeOpt, resultKind, originalUserDefinedOperatorsOpt, type, base.HasErrors);
			boundUnaryOperator.CopyAttributes(this);
			return boundUnaryOperator;
		}
		return this;
	}
}
