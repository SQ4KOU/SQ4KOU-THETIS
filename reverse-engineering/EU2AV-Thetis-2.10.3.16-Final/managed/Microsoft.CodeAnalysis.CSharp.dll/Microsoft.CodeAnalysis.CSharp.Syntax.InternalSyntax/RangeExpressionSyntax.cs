namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RangeExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax? leftOperand;

	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax? rightOperand;

	public ExpressionSyntax? LeftOperand => leftOperand;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax? RightOperand => rightOperand;

	internal RangeExpressionSyntax(SyntaxKind kind, ExpressionSyntax? leftOperand, SyntaxToken operatorToken, ExpressionSyntax? rightOperand, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		if (leftOperand != null)
		{
			AdjustFlagsAndWidth(leftOperand);
			this.leftOperand = leftOperand;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (rightOperand != null)
		{
			AdjustFlagsAndWidth(rightOperand);
			this.rightOperand = rightOperand;
		}
	}

	internal RangeExpressionSyntax(SyntaxKind kind, ExpressionSyntax? leftOperand, SyntaxToken operatorToken, ExpressionSyntax? rightOperand, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		if (leftOperand != null)
		{
			AdjustFlagsAndWidth(leftOperand);
			this.leftOperand = leftOperand;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (rightOperand != null)
		{
			AdjustFlagsAndWidth(rightOperand);
			this.rightOperand = rightOperand;
		}
	}

	internal RangeExpressionSyntax(SyntaxKind kind, ExpressionSyntax? leftOperand, SyntaxToken operatorToken, ExpressionSyntax? rightOperand)
		: base(kind)
	{
		base.SlotCount = 3;
		if (leftOperand != null)
		{
			AdjustFlagsAndWidth(leftOperand);
			this.leftOperand = leftOperand;
		}
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (rightOperand != null)
		{
			AdjustFlagsAndWidth(rightOperand);
			this.rightOperand = rightOperand;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => leftOperand, 
			1 => operatorToken, 
			2 => rightOperand, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RangeExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRangeExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRangeExpression(this);
	}

	public RangeExpressionSyntax Update(ExpressionSyntax leftOperand, SyntaxToken operatorToken, ExpressionSyntax rightOperand)
	{
		if (leftOperand != LeftOperand || operatorToken != OperatorToken || rightOperand != RightOperand)
		{
			RangeExpressionSyntax rangeExpressionSyntax = SyntaxFactory.RangeExpression(leftOperand, operatorToken, rightOperand);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				rangeExpressionSyntax = rangeExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				rangeExpressionSyntax = rangeExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return rangeExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RangeExpressionSyntax(base.Kind, leftOperand, operatorToken, rightOperand, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RangeExpressionSyntax(base.Kind, leftOperand, operatorToken, rightOperand, GetDiagnostics(), annotations);
	}
}
