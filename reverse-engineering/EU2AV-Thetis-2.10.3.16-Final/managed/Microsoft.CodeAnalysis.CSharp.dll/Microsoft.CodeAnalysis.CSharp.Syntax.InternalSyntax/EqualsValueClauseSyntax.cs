namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EqualsValueClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken equalsToken;

	internal readonly ExpressionSyntax value;

	public SyntaxToken EqualsToken => equalsToken;

	public ExpressionSyntax Value => value;

	internal EqualsValueClauseSyntax(SyntaxKind kind, SyntaxToken equalsToken, ExpressionSyntax value, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal EqualsValueClauseSyntax(SyntaxKind kind, SyntaxToken equalsToken, ExpressionSyntax value, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal EqualsValueClauseSyntax(SyntaxKind kind, SyntaxToken equalsToken, ExpressionSyntax value)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => equalsToken, 
			1 => value, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEqualsValueClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEqualsValueClause(this);
	}

	public EqualsValueClauseSyntax Update(SyntaxToken equalsToken, ExpressionSyntax value)
	{
		if (equalsToken != EqualsToken || value != Value)
		{
			EqualsValueClauseSyntax equalsValueClauseSyntax = SyntaxFactory.EqualsValueClause(equalsToken, value);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				equalsValueClauseSyntax = equalsValueClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				equalsValueClauseSyntax = equalsValueClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return equalsValueClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new EqualsValueClauseSyntax(base.Kind, equalsToken, value, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new EqualsValueClauseSyntax(base.Kind, equalsToken, value, GetDiagnostics(), annotations);
	}
}
