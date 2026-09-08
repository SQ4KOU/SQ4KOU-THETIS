namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ThrowExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken throwKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken ThrowKeyword => throwKeyword;

	public ExpressionSyntax Expression => expression;

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal ThrowExpressionSyntax(SyntaxKind kind, SyntaxToken throwKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => throwKeyword, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ThrowExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitThrowExpression(this);
	}

	public ThrowExpressionSyntax Update(SyntaxToken throwKeyword, ExpressionSyntax expression)
	{
		if (throwKeyword != ThrowKeyword || expression != Expression)
		{
			ThrowExpressionSyntax throwExpressionSyntax = SyntaxFactory.ThrowExpression(throwKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				throwExpressionSyntax = throwExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				throwExpressionSyntax = throwExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return throwExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ThrowExpressionSyntax(base.Kind, throwKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ThrowExpressionSyntax(base.Kind, throwKeyword, expression, GetDiagnostics(), annotations);
	}
}
