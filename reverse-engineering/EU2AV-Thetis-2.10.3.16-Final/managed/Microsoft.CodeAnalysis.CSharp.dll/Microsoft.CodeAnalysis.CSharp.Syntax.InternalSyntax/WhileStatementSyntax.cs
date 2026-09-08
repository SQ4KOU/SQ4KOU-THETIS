using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WhileStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken whileKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken WhileKeyword => whileKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	internal WhileStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal WhileStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal WhileStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => whileKeyword, 
			2 => openParenToken, 
			3 => condition, 
			4 => closeParenToken, 
			5 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhileStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public WhileStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (attributeLists != AttributeLists || whileKeyword != WhileKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || statement != Statement)
		{
			WhileStatementSyntax whileStatementSyntax = SyntaxFactory.WhileStatement(attributeLists, whileKeyword, openParenToken, condition, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				whileStatementSyntax = whileStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				whileStatementSyntax = whileStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return whileStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new WhileStatementSyntax(base.Kind, attributeLists, whileKeyword, openParenToken, condition, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new WhileStatementSyntax(base.Kind, attributeLists, whileKeyword, openParenToken, condition, closeParenToken, statement, GetDiagnostics(), annotations);
	}
}
