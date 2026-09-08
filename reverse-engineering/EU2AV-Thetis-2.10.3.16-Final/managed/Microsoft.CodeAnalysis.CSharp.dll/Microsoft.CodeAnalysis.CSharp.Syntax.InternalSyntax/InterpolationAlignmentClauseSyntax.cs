namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InterpolationAlignmentClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken commaToken;

	internal readonly ExpressionSyntax value;

	public SyntaxToken CommaToken => commaToken;

	public ExpressionSyntax Value => value;

	internal InterpolationAlignmentClauseSyntax(SyntaxKind kind, SyntaxToken commaToken, ExpressionSyntax value, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal InterpolationAlignmentClauseSyntax(SyntaxKind kind, SyntaxToken commaToken, ExpressionSyntax value, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal InterpolationAlignmentClauseSyntax(SyntaxKind kind, SyntaxToken commaToken, ExpressionSyntax value)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(commaToken);
		this.commaToken = commaToken;
		AdjustFlagsAndWidth(value);
		this.value = value;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => commaToken, 
			1 => value, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolationAlignmentClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolationAlignmentClause(this);
	}

	public InterpolationAlignmentClauseSyntax Update(SyntaxToken commaToken, ExpressionSyntax value)
	{
		if (commaToken != CommaToken || value != Value)
		{
			InterpolationAlignmentClauseSyntax interpolationAlignmentClauseSyntax = SyntaxFactory.InterpolationAlignmentClause(commaToken, value);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				interpolationAlignmentClauseSyntax = interpolationAlignmentClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				interpolationAlignmentClauseSyntax = interpolationAlignmentClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return interpolationAlignmentClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new InterpolationAlignmentClauseSyntax(base.Kind, commaToken, value, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new InterpolationAlignmentClauseSyntax(base.Kind, commaToken, value, GetDiagnostics(), annotations);
	}
}
