namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UndefDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken undefKeyword;

	internal readonly SyntaxToken name;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken UndefKeyword => undefKeyword;

	public SyntaxToken Name => name;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal UndefDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(undefKeyword);
		this.undefKeyword = undefKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal UndefDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(undefKeyword);
		this.undefKeyword = undefKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal UndefDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(undefKeyword);
		this.undefKeyword = undefKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => undefKeyword, 
			2 => name, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUndefDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUndefDirectiveTrivia(this);
	}

	public UndefDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || undefKeyword != UndefKeyword || name != Name || endOfDirectiveToken != EndOfDirectiveToken)
		{
			UndefDirectiveTriviaSyntax undefDirectiveTriviaSyntax = SyntaxFactory.UndefDirectiveTrivia(hashToken, undefKeyword, name, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				undefDirectiveTriviaSyntax = undefDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				undefDirectiveTriviaSyntax = undefDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return undefDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new UndefDirectiveTriviaSyntax(base.Kind, hashToken, undefKeyword, name, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new UndefDirectiveTriviaSyntax(base.Kind, hashToken, undefKeyword, name, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
