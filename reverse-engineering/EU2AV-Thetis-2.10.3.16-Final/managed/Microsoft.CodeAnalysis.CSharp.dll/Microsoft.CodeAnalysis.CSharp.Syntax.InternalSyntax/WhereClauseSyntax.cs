namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WhereClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken whereKeyword;

	internal readonly ExpressionSyntax condition;

	public SyntaxToken WhereKeyword => whereKeyword;

	public ExpressionSyntax Condition => condition;

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => whereKeyword, 
			1 => condition, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WhereClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhereClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhereClause(this);
	}

	public WhereClauseSyntax Update(SyntaxToken whereKeyword, ExpressionSyntax condition)
	{
		if (whereKeyword != WhereKeyword || condition != Condition)
		{
			WhereClauseSyntax whereClauseSyntax = SyntaxFactory.WhereClause(whereKeyword, condition);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				whereClauseSyntax = whereClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				whereClauseSyntax = whereClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return whereClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new WhereClauseSyntax(base.Kind, whereKeyword, condition, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new WhereClauseSyntax(base.Kind, whereKeyword, condition, GetDiagnostics(), annotations);
	}
}
