namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IgnoredDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken colonToken;

	internal readonly SyntaxToken? content;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ColonToken => colonToken;

	public SyntaxToken? Content => content;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal IgnoredDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken colonToken, SyntaxToken? content, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal IgnoredDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken colonToken, SyntaxToken? content, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal IgnoredDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken colonToken, SyntaxToken? content, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
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
			1 => colonToken, 
			2 => content, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IgnoredDirectiveTriviaSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIgnoredDirectiveTrivia(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIgnoredDirectiveTrivia(this);
	}

	public IgnoredDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken colonToken, SyntaxToken content, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || colonToken != ColonToken || content != Content || endOfDirectiveToken != EndOfDirectiveToken)
		{
			IgnoredDirectiveTriviaSyntax ignoredDirectiveTriviaSyntax = SyntaxFactory.IgnoredDirectiveTrivia(hashToken, colonToken, content, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				ignoredDirectiveTriviaSyntax = ignoredDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				ignoredDirectiveTriviaSyntax = ignoredDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return ignoredDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new IgnoredDirectiveTriviaSyntax(base.Kind, hashToken, colonToken, content, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new IgnoredDirectiveTriviaSyntax(base.Kind, hashToken, colonToken, content, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}
}
