using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUserDefinedConditionalLogicalOperator : BoundBinaryOperatorBase
{
	public override Symbol ExpressionSymbol => LogicalOperator;

	public BinaryOperatorKind OperatorKind { get; }

	public MethodSymbol LogicalOperator { get; }

	public MethodSymbol TrueOperator { get; }

	public MethodSymbol FalseOperator { get; }

	public BoundValuePlaceholder? TrueFalseOperandPlaceholder { get; }

	public BoundExpression? TrueFalseOperandConversion { get; }

	public TypeSymbol? ConstrainedToTypeOpt { get; }

	public override LookupResultKind ResultKind { get; }

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BoundUserDefinedConditionalLogicalOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, MethodSymbol logicalOperator, MethodSymbol trueOperator, MethodSymbol falseOperator, BoundValuePlaceholder? trueFalseOperandPlaceholder, BoundExpression? trueFalseOperandConversion, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, BoundExpression left, BoundExpression right, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.UserDefinedConditionalLogicalOperator, syntax, left, right, type, hasErrors || trueFalseOperandPlaceholder.HasErrors() || trueFalseOperandConversion.HasErrors() || left.HasErrors() || right.HasErrors())
	{
		OperatorKind = operatorKind;
		LogicalOperator = logicalOperator;
		TrueOperator = trueOperator;
		FalseOperator = falseOperator;
		TrueFalseOperandPlaceholder = trueFalseOperandPlaceholder;
		TrueFalseOperandConversion = trueFalseOperandConversion;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
		ResultKind = resultKind;
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUserDefinedConditionalLogicalOperator(this);
	}

	public BoundUserDefinedConditionalLogicalOperator Update(BinaryOperatorKind operatorKind, MethodSymbol logicalOperator, MethodSymbol trueOperator, MethodSymbol falseOperator, BoundValuePlaceholder? trueFalseOperandPlaceholder, BoundExpression? trueFalseOperandConversion, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(logicalOperator, LogicalOperator) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(trueOperator, TrueOperator) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(falseOperator, FalseOperator) || trueFalseOperandPlaceholder != TrueFalseOperandPlaceholder || trueFalseOperandConversion != TrueFalseOperandConversion || !TypeSymbol.Equals(constrainedToTypeOpt, ConstrainedToTypeOpt, TypeCompareKind.ConsiderEverything) || resultKind != ResultKind || originalUserDefinedOperatorsOpt != OriginalUserDefinedOperatorsOpt || left != base.Left || right != base.Right || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = new BoundUserDefinedConditionalLogicalOperator(Syntax, operatorKind, logicalOperator, trueOperator, falseOperator, trueFalseOperandPlaceholder, trueFalseOperandConversion, constrainedToTypeOpt, resultKind, originalUserDefinedOperatorsOpt, left, right, type, base.HasErrors);
			boundUserDefinedConditionalLogicalOperator.CopyAttributes(this);
			return boundUserDefinedConditionalLogicalOperator;
		}
		return this;
	}
}
