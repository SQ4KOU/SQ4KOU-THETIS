using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIncrementOperator : BoundExpression
{
	public override Symbol? ExpressionSymbol => MethodOpt;

	public new TypeSymbol Type => base.Type;

	public UnaryOperatorKind OperatorKind { get; }

	public BoundExpression Operand { get; }

	public MethodSymbol? MethodOpt { get; }

	public TypeSymbol? ConstrainedToTypeOpt { get; }

	public BoundValuePlaceholder? OperandPlaceholder { get; }

	public BoundExpression? OperandConversion { get; }

	public BoundValuePlaceholder? ResultPlaceholder { get; }

	public BoundExpression? ResultConversion { get; }

	public override LookupResultKind ResultKind { get; }

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BoundIncrementOperator(CSharpSyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, BoundValuePlaceholder? resultPlaceholder, BoundExpression? resultConversion, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, operand, methodOpt, constrainedToTypeOpt, operandPlaceholder, operandConversion, resultPlaceholder, resultConversion, resultKind, default(ImmutableArray<MethodSymbol>), type, hasErrors)
	{
	}

	public BoundIncrementOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, BoundValuePlaceholder? resultPlaceholder, BoundExpression? resultConversion, LookupResultKind resultKind, TypeSymbol type)
	{
		return Update(operatorKind, operand, methodOpt, constrainedToTypeOpt, operandPlaceholder, operandConversion, resultPlaceholder, resultConversion, resultKind, OriginalUserDefinedOperatorsOpt, type);
	}

	public BoundIncrementOperator(SyntaxNode syntax, UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, BoundValuePlaceholder? resultPlaceholder, BoundExpression? resultConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IncrementOperator, syntax, type, hasErrors || operand.HasErrors() || operandPlaceholder.HasErrors() || operandConversion.HasErrors() || resultPlaceholder.HasErrors() || resultConversion.HasErrors())
	{
		OperatorKind = operatorKind;
		Operand = operand;
		MethodOpt = methodOpt;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
		OperandPlaceholder = operandPlaceholder;
		OperandConversion = operandConversion;
		ResultPlaceholder = resultPlaceholder;
		ResultConversion = resultConversion;
		ResultKind = resultKind;
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIncrementOperator(this);
	}

	public BoundIncrementOperator Update(UnaryOperatorKind operatorKind, BoundExpression operand, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, BoundValuePlaceholder? operandPlaceholder, BoundExpression? operandConversion, BoundValuePlaceholder? resultPlaceholder, BoundExpression? resultConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || operand != Operand || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(methodOpt, MethodOpt) || !TypeSymbol.Equals(constrainedToTypeOpt, ConstrainedToTypeOpt, TypeCompareKind.ConsiderEverything) || operandPlaceholder != OperandPlaceholder || operandConversion != OperandConversion || resultPlaceholder != ResultPlaceholder || resultConversion != ResultConversion || resultKind != ResultKind || originalUserDefinedOperatorsOpt != OriginalUserDefinedOperatorsOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundIncrementOperator boundIncrementOperator = new BoundIncrementOperator(Syntax, operatorKind, operand, methodOpt, constrainedToTypeOpt, operandPlaceholder, operandConversion, resultPlaceholder, resultConversion, resultKind, originalUserDefinedOperatorsOpt, type, base.HasErrors);
			boundIncrementOperator.CopyAttributes(this);
			return boundIncrementOperator;
		}
		return this;
	}
}
