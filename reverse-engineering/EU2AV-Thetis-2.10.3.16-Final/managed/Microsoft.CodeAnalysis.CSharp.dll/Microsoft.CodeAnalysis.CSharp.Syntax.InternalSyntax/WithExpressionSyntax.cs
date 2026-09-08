namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WithExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken withKeyword;

	internal readonly InitializerExpressionSyntax initializer;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken WithKeyword => withKeyword;

	public InitializerExpressionSyntax Initializer => initializer;

	internal WithExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken withKeyword, InitializerExpressionSyntax initializer, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(withKeyword);
		this.withKeyword = withKeyword;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal WithExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken withKeyword, InitializerExpressionSyntax initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(withKeyword);
		this.withKeyword = withKeyword;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal WithExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken withKeyword, InitializerExpressionSyntax initializer)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(withKeyword);
		this.withKeyword = withKeyword;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => expression, 
			1 => withKeyword, 
			2 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WithExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWithExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWithExpression(this);
	}

	public WithExpressionSyntax Update(ExpressionSyntax expression, SyntaxToken withKeyword, InitializerExpressionSyntax initializer)
	{
		if (expression != Expression || withKeyword != WithKeyword || initializer != Initializer)
		{
			WithExpressionSyntax withExpressionSyntax = SyntaxFactory.WithExpression(expression, withKeyword, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				withExpressionSyntax = withExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				withExpressionSyntax = withExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return withExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new WithExpressionSyntax(base.Kind, expression, withKeyword, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new WithExpressionSyntax(base.Kind, expression, withKeyword, initializer, GetDiagnostics(), annotations);
	}
}
