namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CastExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken closeParenToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken OpenParenToken => openParenToken;

	public TypeSyntax Type => type;

	public SyntaxToken CloseParenToken => closeParenToken;

	public ExpressionSyntax Expression => expression;

	internal CastExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal CastExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal CastExpressionSyntax(SyntaxKind kind, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => type, 
			2 => closeParenToken, 
			3 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCastExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCastExpression(this);
	}

	public CastExpressionSyntax Update(SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, ExpressionSyntax expression)
	{
		if (openParenToken != OpenParenToken || type != Type || closeParenToken != CloseParenToken || expression != Expression)
		{
			CastExpressionSyntax castExpressionSyntax = SyntaxFactory.CastExpression(openParenToken, type, closeParenToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				castExpressionSyntax = castExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				castExpressionSyntax = castExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return castExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CastExpressionSyntax(base.Kind, openParenToken, type, closeParenToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CastExpressionSyntax(base.Kind, openParenToken, type, closeParenToken, expression, GetDiagnostics(), annotations);
	}
}
