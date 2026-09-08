namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RefValueExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken comma;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken Keyword => keyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken Comma => comma;

	public TypeSyntax Type => type;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal RefValueExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 6;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(comma);
		this.comma = comma;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal RefValueExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 6;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(comma);
		this.comma = comma;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal RefValueExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 6;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(comma);
		this.comma = comma;
		AdjustFlagsAndWidth(type);
		this.type = type;
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
			3 => comma, 
			4 => type, 
			5 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RefValueExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefValueExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefValueExpression(this);
	}

	public RefValueExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken comma, TypeSyntax type, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || expression != Expression || comma != Comma || type != Type || closeParenToken != CloseParenToken)
		{
			RefValueExpressionSyntax refValueExpressionSyntax = SyntaxFactory.RefValueExpression(keyword, openParenToken, expression, comma, type, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				refValueExpressionSyntax = refValueExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				refValueExpressionSyntax = refValueExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return refValueExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RefValueExpressionSyntax(base.Kind, keyword, openParenToken, expression, comma, type, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RefValueExpressionSyntax(base.Kind, keyword, openParenToken, expression, comma, type, closeParenToken, GetDiagnostics(), annotations);
	}
}
