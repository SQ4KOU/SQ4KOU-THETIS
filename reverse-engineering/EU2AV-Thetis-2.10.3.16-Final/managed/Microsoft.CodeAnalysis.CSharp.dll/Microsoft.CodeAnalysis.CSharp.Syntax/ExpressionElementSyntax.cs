using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExpressionElementSyntax : CollectionElementSyntax
{
	private ExpressionSyntax? expression;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	internal ExpressionElementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return GetRedAtZero(ref expression);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return expression;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExpressionElement(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitExpressionElement(this);
	}

	public ExpressionElementSyntax Update(ExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			ExpressionElementSyntax expressionElementSyntax = SyntaxFactory.ExpressionElement(expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return expressionElementSyntax;
			}
			return expressionElementSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ExpressionElementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression);
	}
}
