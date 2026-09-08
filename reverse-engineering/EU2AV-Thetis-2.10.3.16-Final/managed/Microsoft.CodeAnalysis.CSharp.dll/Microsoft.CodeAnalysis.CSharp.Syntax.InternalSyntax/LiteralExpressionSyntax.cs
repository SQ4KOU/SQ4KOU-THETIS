namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LiteralExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken token;

	public SyntaxToken Token => token;

	internal LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken token, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken token, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken token)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return token;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLiteralExpression(this);
	}

	public LiteralExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			LiteralExpressionSyntax literalExpressionSyntax = SyntaxFactory.LiteralExpression(base.Kind, token);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				literalExpressionSyntax = literalExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				literalExpressionSyntax = literalExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return literalExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LiteralExpressionSyntax(base.Kind, token, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LiteralExpressionSyntax(base.Kind, token, GetDiagnostics(), annotations);
	}
}
