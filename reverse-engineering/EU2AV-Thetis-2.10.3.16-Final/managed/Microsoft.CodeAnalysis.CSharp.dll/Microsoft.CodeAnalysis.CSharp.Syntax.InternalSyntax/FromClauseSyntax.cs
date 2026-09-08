namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FromClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken fromKeyword;

	internal readonly TypeSyntax? type;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax expression;

	public SyntaxToken FromKeyword => fromKeyword;

	public TypeSyntax? Type => type;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken InKeyword => inKeyword;

	public ExpressionSyntax Expression => expression;

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal FromClauseSyntax(SyntaxKind kind, SyntaxToken fromKeyword, TypeSyntax? type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(fromKeyword);
		this.fromKeyword = fromKeyword;
		if (type != null)
		{
			AdjustFlagsAndWidth(type);
			this.type = type;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => fromKeyword, 
			1 => type, 
			2 => identifier, 
			3 => inKeyword, 
			4 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FromClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFromClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFromClause(this);
	}

	public FromClauseSyntax Update(SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
	{
		if (fromKeyword != FromKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || expression != Expression)
		{
			FromClauseSyntax fromClauseSyntax = SyntaxFactory.FromClause(fromKeyword, type, identifier, inKeyword, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fromClauseSyntax = fromClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fromClauseSyntax = fromClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return fromClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FromClauseSyntax(base.Kind, fromKeyword, type, identifier, inKeyword, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FromClauseSyntax(base.Kind, fromKeyword, type, identifier, inKeyword, expression, GetDiagnostics(), annotations);
	}
}
