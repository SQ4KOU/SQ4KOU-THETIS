namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SpreadElementSyntax : CollectionElementSyntax
{
	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax Expression => expression;

	internal SpreadElementSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SpreadElementSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal SpreadElementSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax expression)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SpreadElementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSpreadElement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSpreadElement(this);
	}

	public SpreadElementSyntax Update(SyntaxToken operatorToken, ExpressionSyntax expression)
	{
		if (operatorToken != OperatorToken || expression != Expression)
		{
			SpreadElementSyntax spreadElementSyntax = SyntaxFactory.SpreadElement(operatorToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				spreadElementSyntax = spreadElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				spreadElementSyntax = spreadElementSyntax.WithAnnotationsGreen(annotations);
			}
			return spreadElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SpreadElementSyntax(base.Kind, operatorToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SpreadElementSyntax(base.Kind, operatorToken, expression, GetDiagnostics(), annotations);
	}
}
