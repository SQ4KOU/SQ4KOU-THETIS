using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LockStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken lockKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken LockKeyword => lockKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	internal LockStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(lockKeyword);
		this.lockKeyword = lockKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LockStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(lockKeyword);
		this.lockKeyword = lockKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LockStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 6;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(lockKeyword);
		this.lockKeyword = lockKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
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
			1 => lockKeyword, 
			2 => openParenToken, 
			3 => expression, 
			4 => closeParenToken, 
			5 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLockStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLockStatement(this);
	}

	public LockStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken lockKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (attributeLists != AttributeLists || lockKeyword != LockKeyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			LockStatementSyntax lockStatementSyntax = SyntaxFactory.LockStatement(attributeLists, lockKeyword, openParenToken, expression, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				lockStatementSyntax = lockStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				lockStatementSyntax = lockStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return lockStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new LockStatementSyntax(base.Kind, attributeLists, lockKeyword, openParenToken, expression, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new LockStatementSyntax(base.Kind, attributeLists, lockKeyword, openParenToken, expression, closeParenToken, statement, GetDiagnostics(), annotations);
	}
}
