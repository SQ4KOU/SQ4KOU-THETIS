using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LineDirectiveTriviaSyntax : LineOrSpanDirectiveTriviaSyntax
{
	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).hashToken, base.Position, 0);

	public override SyntaxToken LineKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).lineKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken Line => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).line, GetChildPosition(2), GetChildIndex(2));

	public override SyntaxToken File
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken file = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).file;
			if (file == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, file, GetChildPosition(3), GetChildIndex(3));
		}
	}

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).endOfDirectiveToken, GetChildPosition(4), GetChildIndex(4));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineDirectiveTriviaSyntax)base.Green).IsActive;

	internal LineDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitLineDirectiveTrivia(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitLineDirectiveTrivia(this);
	}

	public LineDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || lineKeyword != LineKeyword || line != Line || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LineDirectiveTriviaSyntax lineDirectiveTriviaSyntax = SyntaxFactory.LineDirectiveTrivia(hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return lineDirectiveTriviaSyntax;
			}
			return lineDirectiveTriviaSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new LineDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, LineKeyword, Line, File, EndOfDirectiveToken, IsActive);
	}

	internal override LineOrSpanDirectiveTriviaSyntax WithLineKeywordCore(SyntaxToken lineKeyword)
	{
		return WithLineKeyword(lineKeyword);
	}

	public new LineDirectiveTriviaSyntax WithLineKeyword(SyntaxToken lineKeyword)
	{
		return Update(HashToken, lineKeyword, Line, File, EndOfDirectiveToken, IsActive);
	}

	public LineDirectiveTriviaSyntax WithLine(SyntaxToken line)
	{
		return Update(HashToken, LineKeyword, line, File, EndOfDirectiveToken, IsActive);
	}

	internal override LineOrSpanDirectiveTriviaSyntax WithFileCore(SyntaxToken file)
	{
		return WithFile(file);
	}

	public new LineDirectiveTriviaSyntax WithFile(SyntaxToken file)
	{
		return Update(HashToken, LineKeyword, Line, file, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new LineDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, LineKeyword, Line, File, endOfDirectiveToken, IsActive);
	}

	public LineDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, LineKeyword, Line, File, EndOfDirectiveToken, isActive);
	}
}
