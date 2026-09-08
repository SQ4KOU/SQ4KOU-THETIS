using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DiscardPatternSyntax : PatternSyntax
{
	public SyntaxToken UnderscoreToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DiscardPatternSyntax)base.Green).underscoreToken, base.Position, 0);

	internal DiscardPatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitDiscardPattern(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitDiscardPattern(this);
	}

	public DiscardPatternSyntax Update(SyntaxToken underscoreToken)
	{
		if (underscoreToken != UnderscoreToken)
		{
			DiscardPatternSyntax discardPatternSyntax = SyntaxFactory.DiscardPattern(underscoreToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return discardPatternSyntax;
			}
			return discardPatternSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public DiscardPatternSyntax WithUnderscoreToken(SyntaxToken underscoreToken)
	{
		return Update(underscoreToken);
	}
}
