namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LetClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken letKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken equalsToken;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken LetKeyword => letKeyword;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken EqualsToken => equalsToken;

	public ExpressionSyntax Expression => expression;

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal LetClauseSyntax(SyntaxKind kind, SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(letKeyword);
		this.letKeyword = letKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => letKeyword, 
			1 => identifier, 
			2 => equalsToken, 
			3 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LetClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLetClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLetClause(this);
	}

	public LetClauseSyntax Update(SyntaxToken letKeyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax expression)
	{
		if (letKeyword != LetKeyword || identifier != Identifier || equalsToken != EqualsToken || expression != Expression)
		{
			LetClauseSyntax letClauseSyntax = SyntaxFactory.LetClause(letKeyword, identifier, equalsToken, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				letClauseSyntax = letClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				letClauseSyntax = letClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return letClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LetClauseSyntax(base.Kind, letKeyword, identifier, equalsToken, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LetClauseSyntax(base.Kind, letKeyword, identifier, equalsToken, expression, GetDiagnostics(), annotations);
	}
}
