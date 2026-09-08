namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExpressionColonSyntax : BaseExpressionColonSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken colonToken;

	public override ExpressionSyntax Expression => expression;

	public override SyntaxToken ColonToken => colonToken;

	internal ExpressionColonSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken colonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal ExpressionColonSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal ExpressionColonSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken colonToken)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => expression, 
			1 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionColonSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExpressionColon(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExpressionColon(this);
	}

	public ExpressionColonSyntax Update(ExpressionSyntax expression, SyntaxToken colonToken)
	{
		if (expression != Expression || colonToken != ColonToken)
		{
			ExpressionColonSyntax expressionColonSyntax = SyntaxFactory.ExpressionColon(expression, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				expressionColonSyntax = expressionColonSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				expressionColonSyntax = expressionColonSyntax.WithAnnotationsGreen(annotations);
			}
			return expressionColonSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ExpressionColonSyntax(base.Kind, expression, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ExpressionColonSyntax(base.Kind, expression, colonToken, GetDiagnostics(), annotations);
	}
}
