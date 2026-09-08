namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class MemberAccessExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken operatorToken;

	internal readonly SimpleNameSyntax name;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken OperatorToken => operatorToken;

	public SimpleNameSyntax Name => name;

	internal MemberAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal MemberAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal MemberAccessExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => expression, 
			1 => operatorToken, 
			2 => name, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAccessExpression(this);
	}

	public MemberAccessExpressionSyntax Update(ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		if (expression != Expression || operatorToken != OperatorToken || name != Name)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(base.Kind, expression, operatorToken, name);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				memberAccessExpressionSyntax = memberAccessExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				memberAccessExpressionSyntax = memberAccessExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return memberAccessExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new MemberAccessExpressionSyntax(base.Kind, expression, operatorToken, name, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new MemberAccessExpressionSyntax(base.Kind, expression, operatorToken, name, GetDiagnostics(), annotations);
	}
}
