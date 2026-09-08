using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUnconvertedConditionalOperator : BoundExpression
{
	public override object Display
	{
		get
		{
			if ((object)Type != null)
			{
				return base.Display;
			}
			return MessageID.IDS_FeatureTargetTypedConditional.Localize();
		}
	}

	public new TypeSymbol? Type => base.Type;

	public BoundExpression Condition { get; }

	public BoundExpression Consequence { get; }

	public BoundExpression Alternative { get; }

	public override ConstantValue? ConstantValueOpt { get; }

	public ErrorCode NoCommonTypeError { get; }

	public BoundUnconvertedConditionalOperator(SyntaxNode syntax, BoundExpression condition, BoundExpression consequence, BoundExpression alternative, ConstantValue? constantValueOpt, ErrorCode noCommonTypeError, bool hasErrors = false)
		: base(BoundKind.UnconvertedConditionalOperator, syntax, null, hasErrors || condition.HasErrors() || consequence.HasErrors() || alternative.HasErrors())
	{
		Condition = condition;
		Consequence = consequence;
		Alternative = alternative;
		ConstantValueOpt = constantValueOpt;
		NoCommonTypeError = noCommonTypeError;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnconvertedConditionalOperator(this);
	}

	public BoundUnconvertedConditionalOperator Update(BoundExpression condition, BoundExpression consequence, BoundExpression alternative, ConstantValue? constantValueOpt, ErrorCode noCommonTypeError)
	{
		if (condition != Condition || consequence != Consequence || alternative != Alternative || constantValueOpt != ConstantValueOpt || noCommonTypeError != NoCommonTypeError)
		{
			BoundUnconvertedConditionalOperator boundUnconvertedConditionalOperator = new BoundUnconvertedConditionalOperator(Syntax, condition, consequence, alternative, constantValueOpt, noCommonTypeError, base.HasErrors);
			boundUnconvertedConditionalOperator.CopyAttributes(this);
			return boundUnconvertedConditionalOperator;
		}
		return this;
	}
}
