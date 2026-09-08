namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DefaultConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly SyntaxToken defaultKeyword;

	public SyntaxToken DefaultKeyword => defaultKeyword;

	internal DefaultConstraintSyntax(SyntaxKind kind, SyntaxToken defaultKeyword, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(defaultKeyword);
		this.defaultKeyword = defaultKeyword;
	}

	internal DefaultConstraintSyntax(SyntaxKind kind, SyntaxToken defaultKeyword, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(defaultKeyword);
		this.defaultKeyword = defaultKeyword;
	}

	internal DefaultConstraintSyntax(SyntaxKind kind, SyntaxToken defaultKeyword)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(defaultKeyword);
		this.defaultKeyword = defaultKeyword;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return defaultKeyword;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DefaultConstraintSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDefaultConstraint(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDefaultConstraint(this);
	}

	public DefaultConstraintSyntax Update(SyntaxToken defaultKeyword)
	{
		if (defaultKeyword != DefaultKeyword)
		{
			DefaultConstraintSyntax defaultConstraintSyntax = SyntaxFactory.DefaultConstraint(defaultKeyword);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				defaultConstraintSyntax = defaultConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				defaultConstraintSyntax = defaultConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return defaultConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DefaultConstraintSyntax(base.Kind, defaultKeyword, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DefaultConstraintSyntax(base.Kind, defaultKeyword, GetDiagnostics(), annotations);
	}
}
