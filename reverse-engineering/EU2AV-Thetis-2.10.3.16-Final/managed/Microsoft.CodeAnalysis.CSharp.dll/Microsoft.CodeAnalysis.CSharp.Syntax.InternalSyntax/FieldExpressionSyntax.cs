namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FieldExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken token;

	public SyntaxToken Token => token;

	internal FieldExpressionSyntax(SyntaxKind kind, SyntaxToken token, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal FieldExpressionSyntax(SyntaxKind kind, SyntaxToken token, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal FieldExpressionSyntax(SyntaxKind kind, SyntaxToken token)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FieldExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFieldExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldExpression(this);
	}

	public FieldExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			FieldExpressionSyntax fieldExpressionSyntax = SyntaxFactory.FieldExpression(token);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fieldExpressionSyntax = fieldExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fieldExpressionSyntax = fieldExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return fieldExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FieldExpressionSyntax(base.Kind, token, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FieldExpressionSyntax(base.Kind, token, GetDiagnostics(), annotations);
	}
}
