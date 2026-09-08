using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCompoundAssignmentOperator : BoundExpression
{
	public override Symbol? ExpressionSymbol => Operator.Method;

	public new TypeSymbol Type => base.Type;

	public BinaryOperatorSignature Operator { get; }

	public BoundExpression Left { get; }

	public BoundExpression Right { get; }

	public BoundValuePlaceholder? LeftPlaceholder { get; }

	public BoundExpression? LeftConversion { get; }

	public BoundValuePlaceholder? FinalPlaceholder { get; }

	public BoundExpression? FinalConversion { get; }

	public override LookupResultKind ResultKind { get; }

	public ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt { get; }

	public BoundCompoundAssignmentOperator(SyntaxNode syntax, BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundValuePlaceholder? finalPlaceholder, BoundExpression? finalConversion, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: this(syntax, @operator, left, right, leftPlaceholder, leftConversion, finalPlaceholder, finalConversion, resultKind, default(ImmutableArray<MethodSymbol>), type, hasErrors)
	{
	}

	public BoundCompoundAssignmentOperator Update(BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundValuePlaceholder? finalPlaceholder, BoundExpression? finalConversion, LookupResultKind resultKind, TypeSymbol type)
	{
		return Update(@operator, left, right, leftPlaceholder, leftConversion, finalPlaceholder, finalConversion, resultKind, OriginalUserDefinedOperatorsOpt, type);
	}

	public BoundCompoundAssignmentOperator(SyntaxNode syntax, BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundValuePlaceholder? finalPlaceholder, BoundExpression? finalConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CompoundAssignmentOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors() || leftPlaceholder.HasErrors() || leftConversion.HasErrors() || finalPlaceholder.HasErrors() || finalConversion.HasErrors())
	{
		Operator = @operator;
		Left = left;
		Right = right;
		LeftPlaceholder = leftPlaceholder;
		LeftConversion = leftConversion;
		FinalPlaceholder = finalPlaceholder;
		FinalConversion = finalConversion;
		ResultKind = resultKind;
		OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCompoundAssignmentOperator(this);
	}

	public BoundCompoundAssignmentOperator Update(BinaryOperatorSignature @operator, BoundExpression left, BoundExpression right, BoundValuePlaceholder? leftPlaceholder, BoundExpression? leftConversion, BoundValuePlaceholder? finalPlaceholder, BoundExpression? finalConversion, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type)
	{
		if (@operator != Operator || left != Left || right != Right || leftPlaceholder != LeftPlaceholder || leftConversion != LeftConversion || finalPlaceholder != FinalPlaceholder || finalConversion != FinalConversion || resultKind != ResultKind || originalUserDefinedOperatorsOpt != OriginalUserDefinedOperatorsOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundCompoundAssignmentOperator boundCompoundAssignmentOperator = new BoundCompoundAssignmentOperator(Syntax, @operator, left, right, leftPlaceholder, leftConversion, finalPlaceholder, finalConversion, resultKind, originalUserDefinedOperatorsOpt, type, base.HasErrors);
			boundCompoundAssignmentOperator.CopyAttributes(this);
			return boundCompoundAssignmentOperator;
		}
		return this;
	}
}
