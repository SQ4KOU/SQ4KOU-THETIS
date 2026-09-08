namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ElseClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken elseKeyword;

	internal readonly StatementSyntax statement;

	public SyntaxToken ElseKeyword => elseKeyword;

	public StatementSyntax Statement => statement;

	internal ElseClauseSyntax(SyntaxKind kind, SyntaxToken elseKeyword, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ElseClauseSyntax(SyntaxKind kind, SyntaxToken elseKeyword, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ElseClauseSyntax(SyntaxKind kind, SyntaxToken elseKeyword, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(elseKeyword);
		this.elseKeyword = elseKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => elseKeyword, 
			1 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElseClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElseClause(this);
	}

	public ElseClauseSyntax Update(SyntaxToken elseKeyword, StatementSyntax statement)
	{
		if (elseKeyword != ElseKeyword || statement != Statement)
		{
			ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(elseKeyword, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				elseClauseSyntax = elseClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				elseClauseSyntax = elseClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return elseClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ElseClauseSyntax(base.Kind, elseKeyword, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ElseClauseSyntax(base.Kind, elseKeyword, statement, GetDiagnostics(), annotations);
	}
}
