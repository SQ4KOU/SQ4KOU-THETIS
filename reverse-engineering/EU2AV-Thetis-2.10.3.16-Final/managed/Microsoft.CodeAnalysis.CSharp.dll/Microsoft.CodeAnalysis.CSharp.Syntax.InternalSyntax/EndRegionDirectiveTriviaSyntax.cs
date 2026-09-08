namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EndRegionDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken endRegionKeyword;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken EndRegionKeyword => endRegionKeyword;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal EndRegionDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endRegionKeyword);
		this.endRegionKeyword = endRegionKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal EndRegionDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endRegionKeyword);
		this.endRegionKeyword = endRegionKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal EndRegionDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endRegionKeyword);
		this.endRegionKeyword = endRegionKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => endRegionKeyword, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EndRegionDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEndRegionDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEndRegionDirectiveTrivia(this);
	}

	public EndRegionDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken endRegionKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || endRegionKeyword != EndRegionKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			EndRegionDirectiveTriviaSyntax endRegionDirectiveTriviaSyntax = SyntaxFactory.EndRegionDirectiveTrivia(hashToken, endRegionKeyword, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				endRegionDirectiveTriviaSyntax = endRegionDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				endRegionDirectiveTriviaSyntax = endRegionDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return endRegionDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new EndRegionDirectiveTriviaSyntax(base.Kind, hashToken, endRegionKeyword, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new EndRegionDirectiveTriviaSyntax(base.Kind, hashToken, endRegionKeyword, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
