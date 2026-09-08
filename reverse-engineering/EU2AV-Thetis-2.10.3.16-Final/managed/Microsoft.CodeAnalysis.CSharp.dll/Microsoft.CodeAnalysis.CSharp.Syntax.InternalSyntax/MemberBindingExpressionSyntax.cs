namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class MemberBindingExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken operatorToken;

	internal readonly SimpleNameSyntax name;

	public SyntaxToken OperatorToken => operatorToken;

	public SimpleNameSyntax Name => name;

	internal MemberBindingExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, SimpleNameSyntax name, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal MemberBindingExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, SimpleNameSyntax name, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal MemberBindingExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, SimpleNameSyntax name)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => operatorToken, 
			1 => name, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.MemberBindingExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitMemberBindingExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberBindingExpression(this);
	}

	public MemberBindingExpressionSyntax Update(SyntaxToken operatorToken, SimpleNameSyntax name)
	{
		if (operatorToken != OperatorToken || name != Name)
		{
			MemberBindingExpressionSyntax memberBindingExpressionSyntax = SyntaxFactory.MemberBindingExpression(operatorToken, name);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				memberBindingExpressionSyntax = memberBindingExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				memberBindingExpressionSyntax = memberBindingExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return memberBindingExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new MemberBindingExpressionSyntax(base.Kind, operatorToken, name, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new MemberBindingExpressionSyntax(base.Kind, operatorToken, name, GetDiagnostics(), annotations);
	}
}
