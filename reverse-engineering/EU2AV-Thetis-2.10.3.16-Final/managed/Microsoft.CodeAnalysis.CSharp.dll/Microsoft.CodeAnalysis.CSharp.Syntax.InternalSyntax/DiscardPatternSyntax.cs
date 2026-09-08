namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DiscardPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken underscoreToken;

	public SyntaxToken UnderscoreToken => underscoreToken;

	internal DiscardPatternSyntax(SyntaxKind kind, SyntaxToken underscoreToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(underscoreToken);
		this.underscoreToken = underscoreToken;
	}

	internal DiscardPatternSyntax(SyntaxKind kind, SyntaxToken underscoreToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(underscoreToken);
		this.underscoreToken = underscoreToken;
	}

	internal DiscardPatternSyntax(SyntaxKind kind, SyntaxToken underscoreToken)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DiscardPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDiscardPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDiscardPattern(this);
	}

	public DiscardPatternSyntax Update(SyntaxToken underscoreToken)
	{
		if (underscoreToken != UnderscoreToken)
		{
			DiscardPatternSyntax discardPatternSyntax = SyntaxFactory.DiscardPattern(underscoreToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				discardPatternSyntax = discardPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				discardPatternSyntax = discardPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return discardPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DiscardPatternSyntax(base.Kind, underscoreToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DiscardPatternSyntax(base.Kind, underscoreToken, GetDiagnostics(), annotations);
	}
}
