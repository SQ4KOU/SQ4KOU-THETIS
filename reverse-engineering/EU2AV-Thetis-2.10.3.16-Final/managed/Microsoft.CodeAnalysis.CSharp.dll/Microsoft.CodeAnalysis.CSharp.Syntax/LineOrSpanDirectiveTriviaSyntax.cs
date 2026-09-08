using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class LineOrSpanDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public abstract SyntaxToken LineKeyword { get; }

	public abstract SyntaxToken File { get; }

	internal LineOrSpanDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	public LineOrSpanDirectiveTriviaSyntax WithLineKeyword(SyntaxToken lineKeyword)
	{
		return WithLineKeywordCore(lineKeyword);
	}

	internal abstract LineOrSpanDirectiveTriviaSyntax WithLineKeywordCore(SyntaxToken lineKeyword);

	public LineOrSpanDirectiveTriviaSyntax WithFile(SyntaxToken file)
	{
		return WithFileCore(file);
	}

	internal abstract LineOrSpanDirectiveTriviaSyntax WithFileCore(SyntaxToken file);

	public new LineOrSpanDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return (LineOrSpanDirectiveTriviaSyntax)WithHashTokenCore(hashToken);
	}

	public new LineOrSpanDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return (LineOrSpanDirectiveTriviaSyntax)WithEndOfDirectiveTokenCore(endOfDirectiveToken);
	}
}
