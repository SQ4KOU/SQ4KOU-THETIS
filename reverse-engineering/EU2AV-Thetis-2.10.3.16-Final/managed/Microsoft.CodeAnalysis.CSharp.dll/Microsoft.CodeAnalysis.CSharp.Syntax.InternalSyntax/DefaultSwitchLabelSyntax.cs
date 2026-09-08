namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DefaultSwitchLabelSyntax : SwitchLabelSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken colonToken;

	public override SyntaxToken Keyword => keyword;

	public override SyntaxToken ColonToken => colonToken;

	internal DefaultSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken colonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal DefaultSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal DefaultSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken colonToken)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => keyword, 
			1 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDefaultSwitchLabel(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDefaultSwitchLabel(this);
	}

	public DefaultSwitchLabelSyntax Update(SyntaxToken keyword, SyntaxToken colonToken)
	{
		if (keyword != Keyword || colonToken != ColonToken)
		{
			DefaultSwitchLabelSyntax defaultSwitchLabelSyntax = SyntaxFactory.DefaultSwitchLabel(keyword, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				defaultSwitchLabelSyntax = defaultSwitchLabelSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				defaultSwitchLabelSyntax = defaultSwitchLabelSyntax.WithAnnotationsGreen(annotations);
			}
			return defaultSwitchLabelSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DefaultSwitchLabelSyntax(base.Kind, keyword, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DefaultSwitchLabelSyntax(base.Kind, keyword, colonToken, GetDiagnostics(), annotations);
	}
}
