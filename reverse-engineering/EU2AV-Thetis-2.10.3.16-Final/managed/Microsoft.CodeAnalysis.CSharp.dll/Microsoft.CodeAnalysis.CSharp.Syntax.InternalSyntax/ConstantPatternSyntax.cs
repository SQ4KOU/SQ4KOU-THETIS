namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConstantPatternSyntax : PatternSyntax
{
	internal readonly ExpressionSyntax expression;

	public ExpressionSyntax Expression => expression;

	internal ConstantPatternSyntax(SyntaxKind kind, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ConstantPatternSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ConstantPatternSyntax(SyntaxKind kind, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return expression;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConstantPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstantPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstantPattern(this);
	}

	public ConstantPatternSyntax Update(ExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			ConstantPatternSyntax constantPatternSyntax = SyntaxFactory.ConstantPattern(expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				constantPatternSyntax = constantPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				constantPatternSyntax = constantPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return constantPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ConstantPatternSyntax(base.Kind, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ConstantPatternSyntax(base.Kind, expression, GetDiagnostics(), annotations);
	}
}
