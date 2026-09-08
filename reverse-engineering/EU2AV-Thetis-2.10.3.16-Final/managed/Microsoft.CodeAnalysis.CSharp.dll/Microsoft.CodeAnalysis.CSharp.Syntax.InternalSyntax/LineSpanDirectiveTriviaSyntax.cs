namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LineSpanDirectiveTriviaSyntax : LineOrSpanDirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken lineKeyword;

	internal readonly LineDirectivePositionSyntax start;

	internal readonly SyntaxToken minusToken;

	internal readonly LineDirectivePositionSyntax end;

	internal readonly SyntaxToken? characterOffset;

	internal readonly SyntaxToken file;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public override SyntaxToken LineKeyword => lineKeyword;

	public LineDirectivePositionSyntax Start => start;

	public SyntaxToken MinusToken => minusToken;

	public LineDirectivePositionSyntax End => end;

	public SyntaxToken? CharacterOffset => characterOffset;

	public override SyntaxToken File => file;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal LineSpanDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, LineDirectivePositionSyntax start, SyntaxToken minusToken, LineDirectivePositionSyntax end, SyntaxToken? characterOffset, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 8;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(start);
		this.start = start;
		AdjustFlagsAndWidth(minusToken);
		this.minusToken = minusToken;
		AdjustFlagsAndWidth(end);
		this.end = end;
		if (characterOffset != null)
		{
			AdjustFlagsAndWidth(characterOffset);
			this.characterOffset = characterOffset;
		}
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LineSpanDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, LineDirectivePositionSyntax start, SyntaxToken minusToken, LineDirectivePositionSyntax end, SyntaxToken? characterOffset, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 8;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(start);
		this.start = start;
		AdjustFlagsAndWidth(minusToken);
		this.minusToken = minusToken;
		AdjustFlagsAndWidth(end);
		this.end = end;
		if (characterOffset != null)
		{
			AdjustFlagsAndWidth(characterOffset);
			this.characterOffset = characterOffset;
		}
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LineSpanDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, LineDirectivePositionSyntax start, SyntaxToken minusToken, LineDirectivePositionSyntax end, SyntaxToken? characterOffset, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 8;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(start);
		this.start = start;
		AdjustFlagsAndWidth(minusToken);
		this.minusToken = minusToken;
		AdjustFlagsAndWidth(end);
		this.end = end;
		if (characterOffset != null)
		{
			AdjustFlagsAndWidth(characterOffset);
			this.characterOffset = characterOffset;
		}
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => lineKeyword, 
			2 => start, 
			3 => minusToken, 
			4 => end, 
			5 => characterOffset, 
			6 => file, 
			7 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LineSpanDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLineSpanDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLineSpanDirectiveTrivia(this);
	}

	public LineSpanDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken lineKeyword, LineDirectivePositionSyntax start, SyntaxToken minusToken, LineDirectivePositionSyntax end, SyntaxToken characterOffset, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || lineKeyword != LineKeyword || start != Start || minusToken != MinusToken || end != End || characterOffset != CharacterOffset || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LineSpanDirectiveTriviaSyntax lineSpanDirectiveTriviaSyntax = SyntaxFactory.LineSpanDirectiveTrivia(hashToken, lineKeyword, start, minusToken, end, characterOffset, file, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				lineSpanDirectiveTriviaSyntax = lineSpanDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				lineSpanDirectiveTriviaSyntax = lineSpanDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return lineSpanDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LineSpanDirectiveTriviaSyntax(base.Kind, hashToken, lineKeyword, start, minusToken, end, characterOffset, file, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LineSpanDirectiveTriviaSyntax(base.Kind, hashToken, lineKeyword, start, minusToken, end, characterOffset, file, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
