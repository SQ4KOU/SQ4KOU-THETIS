namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConditionalExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken questionToken;

	internal readonly ExpressionSyntax whenTrue;

	internal readonly SyntaxToken colonToken;

	internal readonly ExpressionSyntax whenFalse;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken QuestionToken => questionToken;

	public ExpressionSyntax WhenTrue => whenTrue;

	public SyntaxToken ColonToken => colonToken;

	public ExpressionSyntax WhenFalse => whenFalse;

	internal ConditionalExpressionSyntax(SyntaxKind kind, ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
		AdjustFlagsAndWidth(whenTrue);
		this.whenTrue = whenTrue;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(whenFalse);
		this.whenFalse = whenFalse;
	}

	internal ConditionalExpressionSyntax(SyntaxKind kind, ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
		AdjustFlagsAndWidth(whenTrue);
		this.whenTrue = whenTrue;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(whenFalse);
		this.whenFalse = whenFalse;
	}

	internal ConditionalExpressionSyntax(SyntaxKind kind, ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
		AdjustFlagsAndWidth(whenTrue);
		this.whenTrue = whenTrue;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(whenFalse);
		this.whenFalse = whenFalse;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => condition, 
			1 => questionToken, 
			2 => whenTrue, 
			3 => colonToken, 
			4 => whenFalse, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConditionalExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConditionalExpression(this);
	}

	public ConditionalExpressionSyntax Update(ExpressionSyntax condition, SyntaxToken questionToken, ExpressionSyntax whenTrue, SyntaxToken colonToken, ExpressionSyntax whenFalse)
	{
		if (condition != Condition || questionToken != QuestionToken || whenTrue != WhenTrue || colonToken != ColonToken || whenFalse != WhenFalse)
		{
			ConditionalExpressionSyntax conditionalExpressionSyntax = SyntaxFactory.ConditionalExpression(condition, questionToken, whenTrue, colonToken, whenFalse);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				conditionalExpressionSyntax = conditionalExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				conditionalExpressionSyntax = conditionalExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return conditionalExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ConditionalExpressionSyntax(base.Kind, condition, questionToken, whenTrue, colonToken, whenFalse, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ConditionalExpressionSyntax(base.Kind, condition, questionToken, whenTrue, colonToken, whenFalse, GetDiagnostics(), annotations);
	}
}
