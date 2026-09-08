using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class RefStructConstraintSyntax : AllowsConstraintSyntax
{
	public SyntaxToken RefKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefStructConstraintSyntax)base.Green).refKeyword, base.Position, 0);

	public SyntaxToken StructKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefStructConstraintSyntax)base.Green).structKeyword, GetChildPosition(1), GetChildIndex(1));

	internal RefStructConstraintSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitRefStructConstraint(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitRefStructConstraint(this);
	}

	public RefStructConstraintSyntax Update(SyntaxToken refKeyword, SyntaxToken structKeyword)
	{
		if (refKeyword != RefKeyword || structKeyword != StructKeyword)
		{
			RefStructConstraintSyntax refStructConstraintSyntax = SyntaxFactory.RefStructConstraint(refKeyword, structKeyword);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return refStructConstraintSyntax;
			}
			return refStructConstraintSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public RefStructConstraintSyntax WithRefKeyword(SyntaxToken refKeyword)
	{
		return Update(refKeyword, StructKeyword);
	}

	public RefStructConstraintSyntax WithStructKeyword(SyntaxToken structKeyword)
	{
		return Update(RefKeyword, structKeyword);
	}
}
