namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LoadDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken loadKeyword;

	internal readonly SyntaxToken file;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken LoadKeyword => loadKeyword;

	public SyntaxToken File => file;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
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
			1 => loadKeyword, 
			2 => file, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLoadDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLoadDirectiveTrivia(this);
	}

	public LoadDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || loadKeyword != LoadKeyword || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LoadDirectiveTriviaSyntax loadDirectiveTriviaSyntax = SyntaxFactory.LoadDirectiveTrivia(hashToken, loadKeyword, file, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				loadDirectiveTriviaSyntax = loadDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				loadDirectiveTriviaSyntax = loadDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return loadDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LoadDirectiveTriviaSyntax(base.Kind, hashToken, loadKeyword, file, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LoadDirectiveTriviaSyntax(base.Kind, hashToken, loadKeyword, file, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
