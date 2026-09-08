using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SpreadElementSyntax : CollectionElementSyntax
{
	private ExpressionSyntax? expression;

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SpreadElementSyntax)base.Green).operatorToken, base.Position, 0);

	public ExpressionSyntax Expression => GetRed(ref expression, 1);

	internal SpreadElementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitSpreadElement(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitSpreadElement(this);
	}

	public SpreadElementSyntax Update(SyntaxToken operatorToken, ExpressionSyntax expression)
	{
		if (operatorToken != OperatorToken || expression != Expression)
		{
			SpreadElementSyntax spreadElementSyntax = SyntaxFactory.SpreadElement(operatorToken, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return spreadElementSyntax;
			}
			return spreadElementSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public SpreadElementSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(operatorToken, Expression);
	}

	public SpreadElementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(OperatorToken, expression);
	}
}
