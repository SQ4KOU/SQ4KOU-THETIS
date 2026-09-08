namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WarningDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken warningKeyword;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken WarningKeyword => warningKeyword;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal WarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal WarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal WarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => warningKeyword, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WarningDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWarningDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWarningDirectiveTrivia(this);
	}

	public WarningDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken warningKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || warningKeyword != WarningKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			WarningDirectiveTriviaSyntax warningDirectiveTriviaSyntax = SyntaxFactory.WarningDirectiveTrivia(hashToken, warningKeyword, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				warningDirectiveTriviaSyntax = warningDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				warningDirectiveTriviaSyntax = warningDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return warningDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new WarningDirectiveTriviaSyntax(base.Kind, hashToken, warningKeyword, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new WarningDirectiveTriviaSyntax(base.Kind, hashToken, warningKeyword, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
