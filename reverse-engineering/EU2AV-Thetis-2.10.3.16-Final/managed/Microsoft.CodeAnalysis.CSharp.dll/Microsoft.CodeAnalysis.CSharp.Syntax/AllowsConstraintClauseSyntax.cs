using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AllowsConstraintClauseSyntax : TypeParameterConstraintSyntax
{
	private SyntaxNode? constraints;

	public SyntaxToken AllowsKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AllowsConstraintClauseSyntax)base.Green).allowsKeyword, base.Position, 0);

	public SeparatedSyntaxList<AllowsConstraintSyntax> Constraints
	{
		get
		{
			SyntaxNode red = GetRed(ref constraints, 1);
			if (red == null)
			{
				return default(SeparatedSyntaxList<AllowsConstraintSyntax>);
			}
			return new SeparatedSyntaxList<AllowsConstraintSyntax>(red, GetChildIndex(1));
		}
	}

	internal AllowsConstraintClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref constraints, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return constraints;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAllowsConstraintClause(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitAllowsConstraintClause(this);
	}

	public AllowsConstraintClauseSyntax Update(SyntaxToken allowsKeyword, SeparatedSyntaxList<AllowsConstraintSyntax> constraints)
	{
		if (allowsKeyword != AllowsKeyword || constraints != Constraints)
		{
			AllowsConstraintClauseSyntax allowsConstraintClauseSyntax = SyntaxFactory.AllowsConstraintClause(allowsKeyword, constraints);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return allowsConstraintClauseSyntax;
			}
			return allowsConstraintClauseSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public AllowsConstraintClauseSyntax WithAllowsKeyword(SyntaxToken allowsKeyword)
	{
		return Update(allowsKeyword, Constraints);
	}

	public AllowsConstraintClauseSyntax WithConstraints(SeparatedSyntaxList<AllowsConstraintSyntax> constraints)
	{
		return Update(AllowsKeyword, constraints);
	}

	public AllowsConstraintClauseSyntax AddConstraints(params AllowsConstraintSyntax[] items)
	{
		return WithConstraints(Constraints.AddRange(items));
	}
}
