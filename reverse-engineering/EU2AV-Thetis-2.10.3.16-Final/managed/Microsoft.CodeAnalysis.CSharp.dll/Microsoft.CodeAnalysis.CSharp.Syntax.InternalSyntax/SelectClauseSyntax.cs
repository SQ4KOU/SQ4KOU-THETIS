namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SelectClauseSyntax : SelectOrGroupClauseSyntax
{
	internal readonly SyntaxToken selectKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken SelectKeyword => selectKeyword;

	public ExpressionSyntax Expression => expression;

	internal SelectClauseSyntax(SyntaxKind kind, SyntaxToken selectKeyword, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(selectKeyword);
		this.selectKeyword = selectKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SelectClauseSyntax(SyntaxKind kind, SyntaxToken selectKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(selectKeyword);
		this.selectKeyword = selectKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SelectClauseSyntax(SyntaxKind kind, SyntaxToken selectKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(selectKeyword);
		this.selectKeyword = selectKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => selectKeyword, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SelectClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSelectClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSelectClause(this);
	}

	public SelectClauseSyntax Update(SyntaxToken selectKeyword, ExpressionSyntax expression)
	{
		if (selectKeyword != SelectKeyword || expression != Expression)
		{
			SelectClauseSyntax selectClauseSyntax = SyntaxFactory.SelectClause(selectKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				selectClauseSyntax = selectClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				selectClauseSyntax = selectClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return selectClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SelectClauseSyntax(base.Kind, selectKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SelectClauseSyntax(base.Kind, selectKeyword, expression, GetDiagnostics(), annotations);
	}
}
