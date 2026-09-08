namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CatchFilterClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken whenKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax filterExpression;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken WhenKeyword => whenKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax FilterExpression => filterExpression;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CatchFilterClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(filterExpression);
		this.filterExpression = filterExpression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => whenKeyword, 
			1 => openParenToken, 
			2 => filterExpression, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCatchFilterClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCatchFilterClause(this);
	}

	public CatchFilterClauseSyntax Update(SyntaxToken whenKeyword, SyntaxToken openParenToken, ExpressionSyntax filterExpression, SyntaxToken closeParenToken)
	{
		if (whenKeyword != WhenKeyword || openParenToken != OpenParenToken || filterExpression != FilterExpression || closeParenToken != CloseParenToken)
		{
			CatchFilterClauseSyntax catchFilterClauseSyntax = SyntaxFactory.CatchFilterClause(whenKeyword, openParenToken, filterExpression, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				catchFilterClauseSyntax = catchFilterClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				catchFilterClauseSyntax = catchFilterClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return catchFilterClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CatchFilterClauseSyntax(base.Kind, whenKeyword, openParenToken, filterExpression, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CatchFilterClauseSyntax(base.Kind, whenKeyword, openParenToken, filterExpression, closeParenToken, GetDiagnostics(), annotations);
	}
}
