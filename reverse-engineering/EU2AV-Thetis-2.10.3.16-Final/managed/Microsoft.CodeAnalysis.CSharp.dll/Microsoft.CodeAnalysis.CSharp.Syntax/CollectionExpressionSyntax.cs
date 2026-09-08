using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CollectionExpressionSyntax : ExpressionSyntax
{
	private SyntaxNode? elements;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CollectionExpressionSyntax)base.Green).openBracketToken, base.Position, 0);

	public SeparatedSyntaxList<CollectionElementSyntax> Elements
	{
		get
		{
			SyntaxNode red = GetRed(ref elements, 1);
			if (red == null)
			{
				return default(SeparatedSyntaxList<CollectionElementSyntax>);
			}
			return new SeparatedSyntaxList<CollectionElementSyntax>(red, GetChildIndex(1));
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CollectionExpressionSyntax)base.Green).closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal CollectionExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref elements, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return elements;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCollectionExpression(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitCollectionExpression(this);
	}

	public CollectionExpressionSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<CollectionElementSyntax> elements, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || elements != Elements || closeBracketToken != CloseBracketToken)
		{
			CollectionExpressionSyntax collectionExpressionSyntax = SyntaxFactory.CollectionExpression(openBracketToken, elements, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return collectionExpressionSyntax;
			}
			return collectionExpressionSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public CollectionExpressionSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Elements, CloseBracketToken);
	}

	public CollectionExpressionSyntax WithElements(SeparatedSyntaxList<CollectionElementSyntax> elements)
	{
		return Update(OpenBracketToken, elements, CloseBracketToken);
	}

	public CollectionExpressionSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Elements, closeBracketToken);
	}

	public CollectionExpressionSyntax AddElements(params CollectionElementSyntax[] items)
	{
		return WithElements(Elements.AddRange(items));
	}
}
