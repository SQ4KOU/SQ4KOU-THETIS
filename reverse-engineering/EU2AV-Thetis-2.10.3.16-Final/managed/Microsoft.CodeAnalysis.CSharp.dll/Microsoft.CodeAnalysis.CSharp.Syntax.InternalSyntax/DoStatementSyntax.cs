using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class DoStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken doKeyword;

	internal readonly StatementSyntax statement;

	internal readonly SyntaxToken whileKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken closeParenToken;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken DoKeyword => doKeyword;

	public StatementSyntax Statement => statement;

	public SyntaxToken WhileKeyword => whileKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken CloseParenToken => closeParenToken;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal DoStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal DoStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal DoStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(doKeyword);
		this.doKeyword = doKeyword;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		AdjustFlagsAndWidth(whileKeyword);
		this.whileKeyword = whileKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => doKeyword, 
			2 => statement, 
			3 => whileKeyword, 
			4 => openParenToken, 
			5 => condition, 
			6 => closeParenToken, 
			7 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDoStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDoStatement(this);
	}

	public DoStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken doKeyword, StatementSyntax statement, SyntaxToken whileKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || doKeyword != DoKeyword || statement != Statement || whileKeyword != WhileKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || semicolonToken != SemicolonToken)
		{
			DoStatementSyntax doStatementSyntax = SyntaxFactory.DoStatement(attributeLists, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				doStatementSyntax = doStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				doStatementSyntax = doStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return doStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new DoStatementSyntax(base.Kind, attributeLists, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new DoStatementSyntax(base.Kind, attributeLists, doKeyword, statement, whileKeyword, openParenToken, condition, closeParenToken, semicolonToken, GetDiagnostics(), annotations);
	}
}
