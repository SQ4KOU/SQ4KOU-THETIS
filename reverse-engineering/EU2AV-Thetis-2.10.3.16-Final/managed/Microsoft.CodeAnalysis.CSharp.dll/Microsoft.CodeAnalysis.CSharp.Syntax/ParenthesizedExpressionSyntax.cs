using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax? expression;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedExpressionSyntax)base.Green).openParenToken, base.Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedExpressionSyntax)base.Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal ParenthesizedExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref expression, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return expression;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitParenthesizedExpression(this);
	}

	public ParenthesizedExpressionSyntax Update(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken)
		{
			ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = SyntaxFactory.ParenthesizedExpression(openParenToken, expression, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return parenthesizedExpressionSyntax;
			}
			return parenthesizedExpressionSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ParenthesizedExpressionSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Expression, CloseParenToken);
	}

	public ParenthesizedExpressionSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(OpenParenToken, expression, CloseParenToken);
	}

	public ParenthesizedExpressionSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Expression, closeParenToken);
	}
}
