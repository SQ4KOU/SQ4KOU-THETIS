namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DiscardDesignationSyntax : VariableDesignationSyntax
{
	internal readonly SyntaxToken underscoreToken;

	public SyntaxToken UnderscoreToken => underscoreToken;

	internal DiscardDesignationSyntax(SyntaxKind kind, SyntaxToken underscoreToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(underscoreToken);
		this.underscoreToken = underscoreToken;
	}

	internal DiscardDesignationSyntax(SyntaxKind kind, SyntaxToken underscoreToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(underscoreToken);
		this.underscoreToken = underscoreToken;
	}

	internal DiscardDesignationSyntax(SyntaxKind kind, SyntaxToken underscoreToken)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(underscoreToken);
		this.underscoreToken = underscoreToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return underscoreToken;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DiscardDesignationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDiscardDesignation(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDiscardDesignation(this);
	}

	public DiscardDesignationSyntax Update(SyntaxToken underscoreToken)
	{
		if (underscoreToken != UnderscoreToken)
		{
			DiscardDesignationSyntax discardDesignationSyntax = SyntaxFactory.DiscardDesignation(underscoreToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				discardDesignationSyntax = discardDesignationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				discardDesignationSyntax = discardDesignationSyntax.WithAnnotationsGreen(annotations);
			}
			return discardDesignationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DiscardDesignationSyntax(base.Kind, underscoreToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DiscardDesignationSyntax(base.Kind, underscoreToken, GetDiagnostics(), annotations);
	}
}
