using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ForEachStatementSyntax : CommonForEachStatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken? awaitKeyword;

	internal readonly SyntaxToken forEachKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly TypeSyntax type;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override SyntaxToken? AwaitKeyword => awaitKeyword;

	public override SyntaxToken ForEachKeyword => forEachKeyword;

	public override SyntaxToken OpenParenToken => openParenToken;

	public TypeSyntax Type => type;

	public SyntaxToken Identifier => identifier;

	public override SyntaxToken InKeyword => inKeyword;

	public override ExpressionSyntax Expression => expression;

	public override SyntaxToken CloseParenToken => closeParenToken;

	public override StatementSyntax Statement => statement;

	internal ForEachStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 10;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 10;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 10;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (awaitKeyword != null)
		{
			AdjustFlagsAndWidth(awaitKeyword);
			this.awaitKeyword = awaitKeyword;
		}
		AdjustFlagsAndWidth(forEachKeyword);
		this.forEachKeyword = forEachKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(type);
		this.type = type;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
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
			1 => awaitKeyword, 
			2 => forEachKeyword, 
			3 => openParenToken, 
			4 => type, 
			5 => identifier, 
			6 => inKeyword, 
			7 => expression, 
			8 => closeParenToken, 
			9 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForEachStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public ForEachStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (attributeLists != AttributeLists || awaitKeyword != AwaitKeyword || forEachKeyword != ForEachKeyword || openParenToken != OpenParenToken || type != Type || identifier != Identifier || inKeyword != InKeyword || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForEachStatementSyntax forEachStatementSyntax = SyntaxFactory.ForEachStatement(attributeLists, awaitKeyword, forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				forEachStatementSyntax = forEachStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				forEachStatementSyntax = forEachStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return forEachStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ForEachStatementSyntax(base.Kind, attributeLists, awaitKeyword, forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ForEachStatementSyntax(base.Kind, attributeLists, awaitKeyword, forEachKeyword, openParenToken, type, identifier, inKeyword, expression, closeParenToken, statement, GetDiagnostics(), annotations);
	}
}
