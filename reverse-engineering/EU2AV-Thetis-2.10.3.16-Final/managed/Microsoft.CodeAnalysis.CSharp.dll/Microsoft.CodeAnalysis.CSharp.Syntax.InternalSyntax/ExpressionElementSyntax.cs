namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExpressionElementSyntax : CollectionElementSyntax
{
	internal readonly ExpressionSyntax expression;

	public ExpressionSyntax Expression => expression;

	internal ExpressionElementSyntax(SyntaxKind kind, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ExpressionElementSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ExpressionElementSyntax(SyntaxKind kind, ExpressionSyntax expression)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionElementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExpressionElement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionElement(this);
	}

	public ExpressionElementSyntax Update(ExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			ExpressionElementSyntax expressionElementSyntax = SyntaxFactory.ExpressionElement(expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				expressionElementSyntax = expressionElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				expressionElementSyntax = expressionElementSyntax.WithAnnotationsGreen(annotations);
			}
			return expressionElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ExpressionElementSyntax(base.Kind, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ExpressionElementSyntax(base.Kind, expression, GetDiagnostics(), annotations);
	}
}
