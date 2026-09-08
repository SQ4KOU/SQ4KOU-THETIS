using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExpressionColonSyntax : BaseExpressionColonSyntax
{
	private ExpressionSyntax? expression;

	public override ExpressionSyntax Expression => GetRedAtZero(ref expression);

	public override SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExpressionColonSyntax)base.Green).colonToken, GetChildPosition(1), GetChildIndex(1));

	internal ExpressionColonSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitExpressionColon(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitExpressionColon(this);
	}

	public ExpressionColonSyntax Update(ExpressionSyntax expression, SyntaxToken colonToken)
	{
		if (expression != Expression || colonToken != ColonToken)
		{
			ExpressionColonSyntax expressionColonSyntax = SyntaxFactory.ExpressionColon(expression, colonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return expressionColonSyntax;
			}
			return expressionColonSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override BaseExpressionColonSyntax WithExpressionCore(ExpressionSyntax expression)
	{
		return WithExpression(expression);
	}

	public new ExpressionColonSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression, ColonToken);
	}

	internal override BaseExpressionColonSyntax WithColonTokenCore(SyntaxToken colonToken)
	{
		return WithColonToken(colonToken);
	}

	public new ExpressionColonSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Expression, colonToken);
	}
}
