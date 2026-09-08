namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class JoinClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken joinKeyword;

	internal readonly TypeSyntax? type;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax inExpression;

	internal readonly SyntaxToken onKeyword;

	internal readonly ExpressionSyntax leftExpression;

	internal readonly SyntaxToken equalsKeyword;

	internal readonly ExpressionSyntax rightExpression;

	internal readonly JoinIntoClauseSyntax? into;

	public SyntaxToken JoinKeyword => joinKeyword;

	public TypeSyntax? Type => type;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken InKeyword => inKeyword;

	public ExpressionSyntax InExpression => inExpression;

	public SyntaxToken OnKeyword => onKeyword;

	public ExpressionSyntax LeftExpression => leftExpression;

	public SyntaxToken EqualsKeyword => equalsKeyword;

	public ExpressionSyntax RightExpression => rightExpression;

	public JoinIntoClauseSyntax? Into => into;

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax? into, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax? into, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal JoinClauseSyntax(SyntaxKind kind, SyntaxToken joinKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax? into)
		: base(kind)
	{
		base.SlotCount = 10;
		AdjustFlagsAndWidth(joinKeyword);
		this.joinKeyword = joinKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(inExpression);
		this.inExpression = inExpression;
		AdjustFlagsAndWidth(onKeyword);
		this.onKeyword = onKeyword;
		AdjustFlagsAndWidth(leftExpression);
		this.leftExpression = leftExpression;
		AdjustFlagsAndWidth(equalsKeyword);
		this.equalsKeyword = equalsKeyword;
		AdjustFlagsAndWidth(rightExpression);
		this.rightExpression = rightExpression;
		if (into != null)
		{
			AdjustFlagsAndWidth(into);
			this.into = into;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => joinKeyword, 
			1 => type, 
			2 => identifier, 
			3 => inKeyword, 
			4 => inExpression, 
			5 => onKeyword, 
			6 => leftExpression, 
			7 => equalsKeyword, 
			8 => rightExpression, 
			9 => into, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.JoinClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitJoinClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitJoinClause(this);
	}

	public JoinClauseSyntax Update(SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
	{
		if (joinKeyword != JoinKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || inExpression != InExpression || onKeyword != OnKeyword || leftExpression != LeftExpression || equalsKeyword != EqualsKeyword || rightExpression != RightExpression || into != Into)
		{
			JoinClauseSyntax joinClauseSyntax = SyntaxFactory.JoinClause(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				joinClauseSyntax = joinClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				joinClauseSyntax = joinClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return joinClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new JoinClauseSyntax(base.Kind, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new JoinClauseSyntax(base.Kind, joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into, GetDiagnostics(), annotations);
	}
}
