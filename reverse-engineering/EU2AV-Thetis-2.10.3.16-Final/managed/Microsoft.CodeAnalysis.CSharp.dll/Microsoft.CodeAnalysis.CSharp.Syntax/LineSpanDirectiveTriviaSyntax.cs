using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LineSpanDirectiveTriviaSyntax : LineOrSpanDirectiveTriviaSyntax
{
	private LineDirectivePositionSyntax? start;

	private LineDirectivePositionSyntax? end;

	public override SyntaxToken HashToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).hashToken, base.Position, 0);

	public override SyntaxToken LineKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).lineKeyword, GetChildPosition(1), GetChildIndex(1));

	public LineDirectivePositionSyntax Start => GetRed(ref start, 2);

	public SyntaxToken MinusToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).minusToken, GetChildPosition(3), GetChildIndex(3));

	public LineDirectivePositionSyntax End => GetRed(ref end, 4);

	public SyntaxToken CharacterOffset
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken characterOffset = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).characterOffset;
			if (characterOffset == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, characterOffset, GetChildPosition(5), GetChildIndex(5));
		}
	}

	public override SyntaxToken File => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).file, GetChildPosition(6), GetChildIndex(6));

	public override SyntaxToken EndOfDirectiveToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).endOfDirectiveToken, GetChildPosition(7), GetChildIndex(7));

	public override bool IsActive => ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LineSpanDirectiveTriviaSyntax)base.Green).IsActive;

	internal LineSpanDirectiveTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref start, 2), 
			4 => GetRed(ref end, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			2 => start, 
			4 => end, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLineSpanDirectiveTrivia(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitLineSpanDirectiveTrivia(this);
	}

	public LineSpanDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken lineKeyword, LineDirectivePositionSyntax start, SyntaxToken minusToken, LineDirectivePositionSyntax end, SyntaxToken characterOffset, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || lineKeyword != LineKeyword || start != Start || minusToken != MinusToken || end != End || characterOffset != CharacterOffset || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LineSpanDirectiveTriviaSyntax lineSpanDirectiveTriviaSyntax = SyntaxFactory.LineSpanDirectiveTrivia(hashToken, lineKeyword, start, minusToken, end, characterOffset, file, endOfDirectiveToken, isActive);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return lineSpanDirectiveTriviaSyntax;
			}
			return lineSpanDirectiveTriviaSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override DirectiveTriviaSyntax WithHashTokenCore(SyntaxToken hashToken)
	{
		return WithHashToken(hashToken);
	}

	public new LineSpanDirectiveTriviaSyntax WithHashToken(SyntaxToken hashToken)
	{
		return Update(hashToken, LineKeyword, Start, MinusToken, End, CharacterOffset, File, EndOfDirectiveToken, IsActive);
	}

	internal override LineOrSpanDirectiveTriviaSyntax WithLineKeywordCore(SyntaxToken lineKeyword)
	{
		return WithLineKeyword(lineKeyword);
	}

	public new LineSpanDirectiveTriviaSyntax WithLineKeyword(SyntaxToken lineKeyword)
	{
		return Update(HashToken, lineKeyword, Start, MinusToken, End, CharacterOffset, File, EndOfDirectiveToken, IsActive);
	}

	public LineSpanDirectiveTriviaSyntax WithStart(LineDirectivePositionSyntax start)
	{
		return Update(HashToken, LineKeyword, start, MinusToken, End, CharacterOffset, File, EndOfDirectiveToken, IsActive);
	}

	public LineSpanDirectiveTriviaSyntax WithMinusToken(SyntaxToken minusToken)
	{
		return Update(HashToken, LineKeyword, Start, minusToken, End, CharacterOffset, File, EndOfDirectiveToken, IsActive);
	}

	public LineSpanDirectiveTriviaSyntax WithEnd(LineDirectivePositionSyntax end)
	{
		return Update(HashToken, LineKeyword, Start, MinusToken, end, CharacterOffset, File, EndOfDirectiveToken, IsActive);
	}

	public LineSpanDirectiveTriviaSyntax WithCharacterOffset(SyntaxToken characterOffset)
	{
		return Update(HashToken, LineKeyword, Start, MinusToken, End, characterOffset, File, EndOfDirectiveToken, IsActive);
	}

	internal override LineOrSpanDirectiveTriviaSyntax WithFileCore(SyntaxToken file)
	{
		return WithFile(file);
	}

	public new LineSpanDirectiveTriviaSyntax WithFile(SyntaxToken file)
	{
		return Update(HashToken, LineKeyword, Start, MinusToken, End, CharacterOffset, file, EndOfDirectiveToken, IsActive);
	}

	internal override DirectiveTriviaSyntax WithEndOfDirectiveTokenCore(SyntaxToken endOfDirectiveToken)
	{
		return WithEndOfDirectiveToken(endOfDirectiveToken);
	}

	public new LineSpanDirectiveTriviaSyntax WithEndOfDirectiveToken(SyntaxToken endOfDirectiveToken)
	{
		return Update(HashToken, LineKeyword, Start, MinusToken, End, CharacterOffset, File, endOfDirectiveToken, IsActive);
	}

	public LineSpanDirectiveTriviaSyntax WithIsActive(bool isActive)
	{
		return Update(HashToken, LineKeyword, Start, MinusToken, End, CharacterOffset, File, EndOfDirectiveToken, isActive);
	}
}
