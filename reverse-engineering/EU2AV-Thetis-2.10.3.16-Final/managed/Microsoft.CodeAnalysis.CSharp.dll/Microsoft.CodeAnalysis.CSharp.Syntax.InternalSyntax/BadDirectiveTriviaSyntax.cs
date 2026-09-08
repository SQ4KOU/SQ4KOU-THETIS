namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BadDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken Identifier => identifier;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal BadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal BadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal BadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => identifier, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BadDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBadDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBadDirectiveTrivia(this);
	}

	public BadDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || identifier != Identifier || endOfDirectiveToken != EndOfDirectiveToken)
		{
			BadDirectiveTriviaSyntax badDirectiveTriviaSyntax = SyntaxFactory.BadDirectiveTrivia(hashToken, identifier, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				badDirectiveTriviaSyntax = badDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				badDirectiveTriviaSyntax = badDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return badDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new BadDirectiveTriviaSyntax(base.Kind, hashToken, identifier, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new BadDirectiveTriviaSyntax(base.Kind, hashToken, identifier, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
