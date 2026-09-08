namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax left;

	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax right;

	public ExpressionSyntax Left => left;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax Right => right;

	internal AssignmentExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal AssignmentExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal AssignmentExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => left, 
			1 => operatorToken, 
			2 => right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAssignmentExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAssignmentExpression(this);
	}

	public AssignmentExpressionSyntax Update(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			AssignmentExpressionSyntax assignmentExpressionSyntax = SyntaxFactory.AssignmentExpression(base.Kind, left, operatorToken, right);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				assignmentExpressionSyntax = assignmentExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				assignmentExpressionSyntax = assignmentExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return assignmentExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AssignmentExpressionSyntax(base.Kind, left, operatorToken, right, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AssignmentExpressionSyntax(base.Kind, left, operatorToken, right, GetDiagnostics(), annotations);
	}
}
