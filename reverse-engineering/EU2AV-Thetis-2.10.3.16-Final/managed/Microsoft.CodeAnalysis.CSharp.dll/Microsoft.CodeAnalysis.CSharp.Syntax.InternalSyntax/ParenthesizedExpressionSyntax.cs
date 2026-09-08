namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ParenthesizedExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal ParenthesizedExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => expression, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedExpression(this);
	}

	public ParenthesizedExpressionSyntax Update(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken)
		{
			ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = SyntaxFactory.ParenthesizedExpression(openParenToken, expression, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parenthesizedExpressionSyntax = parenthesizedExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parenthesizedExpressionSyntax = parenthesizedExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return parenthesizedExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ParenthesizedExpressionSyntax(base.Kind, openParenToken, expression, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ParenthesizedExpressionSyntax(base.Kind, openParenToken, expression, closeParenToken, GetDiagnostics(), annotations);
	}
}
