namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LineDirectiveTriviaSyntax : LineOrSpanDirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken lineKeyword;

	internal readonly SyntaxToken line;

	internal readonly SyntaxToken? file;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public override SyntaxToken LineKeyword => lineKeyword;

	public SyntaxToken Line => line;

	public override SyntaxToken? File => file;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal LineDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken? file, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(line);
		this.line = line;
		if (file != null)
		{
			AdjustFlagsAndWidth(file);
			this.file = file;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LineDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken? file, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(line);
		this.line = line;
		if (file != null)
		{
			AdjustFlagsAndWidth(file);
			this.file = file;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LineDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken? file, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(lineKeyword);
		this.lineKeyword = lineKeyword;
		AdjustFlagsAndWidth(line);
		this.line = line;
		if (file != null)
		{
			AdjustFlagsAndWidth(file);
			this.file = file;
		}
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
			2 => line, 
			3 => file, 
			4 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LineDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLineDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLineDirectiveTrivia(this);
	}

	public LineDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || lineKeyword != LineKeyword || line != Line || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LineDirectiveTriviaSyntax lineDirectiveTriviaSyntax = SyntaxFactory.LineDirectiveTrivia(hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				lineDirectiveTriviaSyntax = lineDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				lineDirectiveTriviaSyntax = lineDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return lineDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LineDirectiveTriviaSyntax(base.Kind, hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LineDirectiveTriviaSyntax(base.Kind, hashToken, lineKeyword, line, file, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
