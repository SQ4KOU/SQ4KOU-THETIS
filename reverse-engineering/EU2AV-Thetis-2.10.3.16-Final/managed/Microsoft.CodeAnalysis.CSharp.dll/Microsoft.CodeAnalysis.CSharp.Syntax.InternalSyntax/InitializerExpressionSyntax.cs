using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InitializerExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode? expressions;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> Expressions => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(expressions));

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? expressions, SyntaxToken closeBraceToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? expressions, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? expressions, SyntaxToken closeBraceToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => expressions, 
			2 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInitializerExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInitializerExpression(this);
	}

	public InitializerExpressionSyntax Update(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || expressions != Expressions || closeBraceToken != CloseBraceToken)
		{
			InitializerExpressionSyntax initializerExpressionSyntax = SyntaxFactory.InitializerExpression(base.Kind, openBraceToken, expressions, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				initializerExpressionSyntax = initializerExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				initializerExpressionSyntax = initializerExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return initializerExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new InitializerExpressionSyntax(base.Kind, openBraceToken, expressions, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new InitializerExpressionSyntax(base.Kind, openBraceToken, expressions, closeBraceToken, GetDiagnostics(), annotations);
	}
}
