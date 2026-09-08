using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IgnoredDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IgnoredDirectiveTriviaSyntax)base.Green).hashToken, base.Position, 0);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IgnoredDirectiveTriviaSyntax)base.Green).colonToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken Content
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken content = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IgnoredDirectiveTriviaSyntax)base.Green).content;
			if (content == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, content, GetChildPosition(2), GetChildIndex(2));
		}
	}

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IgnoredDirectiveTriviaSyntax)base.Green).endOfDirectiveToken, GetChildPosition(3), GetChildIndex(3));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IgnoredDirectiveTriviaSyntax)base.Green).IsActive;

	internal IgnoredDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitIgnoredDirectiveTrivia(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitIgnoredDirectiveTrivia(this);
	}

	public IgnoredDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken colonToken, SyntaxToken content, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || colonToken != ColonToken || content != Content || endOfDirectiveToken != EndOfDirectiveToken)
		{
			IgnoredDirectiveTriviaSyntax ignoredDirectiveTriviaSyntax = SyntaxFactory.IgnoredDirectiveTrivia(hashToken, colonToken, content, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return ignoredDirectiveTriviaSyntax;
			}
			return ignoredDirectiveTriviaSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new IgnoredDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, ColonToken, Content, EndOfDirectiveToken, IsActive);
	}

	public IgnoredDirectiveTriviaSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(HashToken, colonToken, Content, EndOfDirectiveToken, IsActive);
	}

	public IgnoredDirectiveTriviaSyntax WithContent(SyntaxToken content)
	{
		return Update(HashToken, ColonToken, content, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new IgnoredDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, ColonToken, Content, endOfDirectiveToken, IsActive);
	}

	public IgnoredDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, ColonToken, Content, EndOfDirectiveToken, isActive);
	}
}
