using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FieldExpressionSyntax : ExpressionSyntax
{
	public SyntaxToken Token => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldExpressionSyntax)base.Green).token, base.Position, 0);

	internal FieldExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return null;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFieldExpression(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitFieldExpression(this);
	}

	public FieldExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			FieldExpressionSyntax fieldExpressionSyntax = SyntaxFactory.FieldExpression(token);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return fieldExpressionSyntax;
			}
			return fieldExpressionSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public FieldExpressionSyntax WithToken(SyntaxToken token)
	{
		return Update(token);
	}
}
