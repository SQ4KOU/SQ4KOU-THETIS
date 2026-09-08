namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArrowExpressionClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken arrowToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken ArrowToken => arrowToken;

	public ExpressionSyntax Expression => expression;

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ArrowExpressionClauseSyntax(SyntaxKind kind, SyntaxToken arrowToken, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => arrowToken, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrowExpressionClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArrowExpressionClause(this);
	}

	public ArrowExpressionClauseSyntax Update(SyntaxToken arrowToken, ExpressionSyntax expression)
	{
		if (arrowToken != ArrowToken || expression != Expression)
		{
			ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = SyntaxFactory.ArrowExpressionClause(arrowToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				arrowExpressionClauseSyntax = arrowExpressionClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				arrowExpressionClauseSyntax = arrowExpressionClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return arrowExpressionClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ArrowExpressionClauseSyntax(base.Kind, arrowToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ArrowExpressionClauseSyntax(base.Kind, arrowToken, expression, GetDiagnostics(), annotations);
	}
}
