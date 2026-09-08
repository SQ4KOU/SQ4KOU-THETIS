namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class MakeRefExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken Keyword => keyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal MakeRefExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal MakeRefExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal MakeRefExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
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
			0 => keyword, 
			1 => openParenToken, 
			2 => expression, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.MakeRefExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitMakeRefExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMakeRefExpression(this);
	}

	public MakeRefExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken)
		{
			MakeRefExpressionSyntax makeRefExpressionSyntax = SyntaxFactory.MakeRefExpression(keyword, openParenToken, expression, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				makeRefExpressionSyntax = makeRefExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				makeRefExpressionSyntax = makeRefExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return makeRefExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new MakeRefExpressionSyntax(base.Kind, keyword, openParenToken, expression, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new MakeRefExpressionSyntax(base.Kind, keyword, openParenToken, expression, closeParenToken, GetDiagnostics(), annotations);
	}
}
