namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RelationalPatternSyntax : PatternSyntax
{
	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax Expression => expression;

	internal RelationalPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal RelationalPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal RelationalPatternSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => operatorToken, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RelationalPatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRelationalPattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRelationalPattern(this);
	}

	public RelationalPatternSyntax Update(SyntaxToken operatorToken, ExpressionSyntax expression)
	{
		if (operatorToken != OperatorToken || expression != Expression)
		{
			RelationalPatternSyntax relationalPatternSyntax = SyntaxFactory.RelationalPattern(operatorToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				relationalPatternSyntax = relationalPatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				relationalPatternSyntax = relationalPatternSyntax.WithAnnotationsGreen(annotations);
			}
			return relationalPatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RelationalPatternSyntax(base.Kind, operatorToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RelationalPatternSyntax(base.Kind, operatorToken, expression, GetDiagnostics(), annotations);
	}
}
