namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ErrorDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken errorKeyword;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ErrorKeyword => errorKeyword;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal ErrorDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(errorKeyword);
		this.errorKeyword = errorKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ErrorDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(errorKeyword);
		this.errorKeyword = errorKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ErrorDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(errorKeyword);
		this.errorKeyword = errorKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => errorKeyword, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ErrorDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitErrorDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitErrorDirectiveTrivia(this);
	}

	public ErrorDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken errorKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || errorKeyword != ErrorKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ErrorDirectiveTriviaSyntax errorDirectiveTriviaSyntax = SyntaxFactory.ErrorDirectiveTrivia(hashToken, errorKeyword, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				errorDirectiveTriviaSyntax = errorDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				errorDirectiveTriviaSyntax = errorDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return errorDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ErrorDirectiveTriviaSyntax(base.Kind, hashToken, errorKeyword, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ErrorDirectiveTriviaSyntax(base.Kind, hashToken, errorKeyword, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
