namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ElseDirectiveTriviaSyntax : BranchingDirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken elseKeyword;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	internal readonly bool branchTaken;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ElseKeyword => elseKeyword;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	public override bool BranchTaken => branchTaken;

	internal ElseDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
	}

	internal ElseDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
	}

	internal ElseDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => elseKeyword, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElseDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElseDirectiveTrivia(this);
	}

	public ElseDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
	{
		if (hashToken != HashToken || elseKeyword != ElseKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ElseDirectiveTriviaSyntax elseDirectiveTriviaSyntax = SyntaxFactory.ElseDirectiveTrivia(hashToken, elseKeyword, endOfDirectiveToken, isActive, branchTaken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				elseDirectiveTriviaSyntax = elseDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				elseDirectiveTriviaSyntax = elseDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return elseDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ElseDirectiveTriviaSyntax(base.Kind, hashToken, elseKeyword, endOfDirectiveToken, isActive, branchTaken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ElseDirectiveTriviaSyntax(base.Kind, hashToken, elseKeyword, endOfDirectiveToken, isActive, branchTaken, GetDiagnostics(), annotations);
	}
}
