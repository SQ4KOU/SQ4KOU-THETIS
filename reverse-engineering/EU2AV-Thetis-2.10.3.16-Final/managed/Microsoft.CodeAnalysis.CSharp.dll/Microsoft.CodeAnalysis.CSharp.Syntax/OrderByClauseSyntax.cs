using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OrderByClauseSyntax : QueryClauseSyntax
{
	private SyntaxNode? orderings;

	public SyntaxToken OrderByKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OrderByClauseSyntax)base.Green).orderByKeyword, base.Position, 0);

	public SeparatedSyntaxList<OrderingSyntax> Orderings
	{
		get
		{
			SyntaxNode red = GetRed(ref orderings, 1);
			if (red == null)
			{
				return default(SeparatedSyntaxList<OrderingSyntax>);
			}
			return new SeparatedSyntaxList<OrderingSyntax>(red, GetChildIndex(1));
		}
	}

	internal OrderByClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref orderings, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return orderings;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOrderByClause(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitOrderByClause(this);
	}

	public OrderByClauseSyntax Update(SyntaxToken orderByKeyword, SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		if (orderByKeyword != OrderByKeyword || orderings != Orderings)
		{
			OrderByClauseSyntax orderByClauseSyntax = SyntaxFactory.OrderByClause(orderByKeyword, orderings);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return orderByClauseSyntax;
			}
			return orderByClauseSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public OrderByClauseSyntax WithOrderByKeyword(SyntaxToken orderByKeyword)
	{
		return Update(orderByKeyword, Orderings);
	}

	public OrderByClauseSyntax WithOrderings(SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		return Update(OrderByKeyword, orderings);
	}

	public OrderByClauseSyntax AddOrderings(params OrderingSyntax[] items)
	{
		return WithOrderings(Orderings.AddRange(items));
	}
}
