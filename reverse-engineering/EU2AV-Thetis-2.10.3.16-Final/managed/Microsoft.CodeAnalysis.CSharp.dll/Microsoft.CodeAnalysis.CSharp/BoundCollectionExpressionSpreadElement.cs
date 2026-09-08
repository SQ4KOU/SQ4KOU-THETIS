using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCollectionExpressionSpreadElement : BoundNode
{
	public BoundExpression Expression { get; }

	public BoundCollectionExpressionSpreadExpressionPlaceholder? ExpressionPlaceholder { get; }

	public BoundExpression? Conversion { get; }

	public ForEachEnumeratorInfo? EnumeratorInfoOpt { get; }

	public BoundExpression? LengthOrCount { get; }

	public BoundValuePlaceholder? ElementPlaceholder { get; }

	public BoundStatement? IteratorBody { get; }

	public BoundCollectionExpressionSpreadElement(SyntaxNode syntax, BoundExpression expression, BoundCollectionExpressionSpreadExpressionPlaceholder? expressionPlaceholder, BoundExpression? conversion, ForEachEnumeratorInfo? enumeratorInfoOpt, BoundExpression? lengthOrCount, BoundValuePlaceholder? elementPlaceholder, BoundStatement? iteratorBody, bool hasErrors = false)
		: base(BoundKind.CollectionExpressionSpreadElement, syntax, hasErrors || expression.HasErrors() || expressionPlaceholder.HasErrors() || conversion.HasErrors() || lengthOrCount.HasErrors() || elementPlaceholder.HasErrors() || iteratorBody.HasErrors())
	{
		Expression = expression;
		ExpressionPlaceholder = expressionPlaceholder;
		Conversion = conversion;
		EnumeratorInfoOpt = enumeratorInfoOpt;
		LengthOrCount = lengthOrCount;
		ElementPlaceholder = elementPlaceholder;
		IteratorBody = iteratorBody;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCollectionExpressionSpreadElement(this);
	}

	public BoundCollectionExpressionSpreadElement Update(BoundExpression expression, BoundCollectionExpressionSpreadExpressionPlaceholder? expressionPlaceholder, BoundExpression? conversion, ForEachEnumeratorInfo? enumeratorInfoOpt, BoundExpression? lengthOrCount, BoundValuePlaceholder? elementPlaceholder, BoundStatement? iteratorBody)
	{
		if (expression != Expression || expressionPlaceholder != ExpressionPlaceholder || conversion != Conversion || enumeratorInfoOpt != EnumeratorInfoOpt || lengthOrCount != LengthOrCount || elementPlaceholder != ElementPlaceholder || iteratorBody != IteratorBody)
		{
			BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement = new BoundCollectionExpressionSpreadElement(Syntax, expression, expressionPlaceholder, conversion, enumeratorInfoOpt, lengthOrCount, elementPlaceholder, iteratorBody, base.HasErrors);
			boundCollectionExpressionSpreadElement.CopyAttributes(this);
			return boundCollectionExpressionSpreadElement;
		}
		return this;
	}
}
