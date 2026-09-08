namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeOfExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken Keyword => keyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public TypeSyntax Type => type;

	public SyntaxToken CloseParenToken => closeParenToken;

	internal TypeOfExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal TypeOfExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal TypeOfExpressionSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => keyword, 
			1 => openParenToken, 
			2 => type, 
			3 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeOfExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeOfExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeOfExpression(this);
	}

	public TypeOfExpressionSyntax Update(SyntaxToken keyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken closeParenToken)
	{
		if (keyword != Keyword || openParenToken != OpenParenToken || type != Type || closeParenToken != CloseParenToken)
		{
			TypeOfExpressionSyntax typeOfExpressionSyntax = SyntaxFactory.TypeOfExpression(keyword, openParenToken, type, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeOfExpressionSyntax = typeOfExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeOfExpressionSyntax = typeOfExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return typeOfExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TypeOfExpressionSyntax(base.Kind, keyword, openParenToken, type, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TypeOfExpressionSyntax(base.Kind, keyword, openParenToken, type, closeParenToken, GetDiagnostics(), annotations);
	}
}
