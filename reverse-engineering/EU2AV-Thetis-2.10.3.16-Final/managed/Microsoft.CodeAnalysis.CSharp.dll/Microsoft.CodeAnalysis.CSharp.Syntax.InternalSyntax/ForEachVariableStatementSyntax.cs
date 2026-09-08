using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ForEachVariableStatementSyntax : CommonForEachStatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken? awaitKeyword;

	internal readonly SyntaxToken forEachKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax variable;

	internal readonly SyntaxToken inKeyword;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override SyntaxToken? AwaitKeyword => awaitKeyword;

	public override SyntaxToken ForEachKeyword => forEachKeyword;

	public override SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Variable => variable;

	public override SyntaxToken InKeyword => inKeyword;

	public override ExpressionSyntax Expression => expression;

	public override SyntaxToken CloseParenToken => closeParenToken;

	public override StatementSyntax Statement => statement;

	internal ForEachVariableStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachVariableStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
		AdjustFlagsAndWidth(inKeyword);
		this.inKeyword = inKeyword;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal ForEachVariableStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken? awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
		: base(kind)
	{
		base.SlotCount = 9;
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
		AdjustFlagsAndWidth(variable);
		this.variable = variable;
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
			4 => variable, 
			5 => inKeyword, 
			6 => expression, 
			7 => closeParenToken, 
			8 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForEachVariableStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachVariableStatement(this);
	}

	public ForEachVariableStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken awaitKeyword, SyntaxToken forEachKeyword, SyntaxToken openParenToken, ExpressionSyntax variable, SyntaxToken inKeyword, ExpressionSyntax expression, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (attributeLists != AttributeLists || awaitKeyword != AwaitKeyword || forEachKeyword != ForEachKeyword || openParenToken != OpenParenToken || variable != Variable || inKeyword != InKeyword || expression != Expression || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = SyntaxFactory.ForEachVariableStatement(attributeLists, awaitKeyword, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				forEachVariableStatementSyntax = forEachVariableStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				forEachVariableStatementSyntax = forEachVariableStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return forEachVariableStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ForEachVariableStatementSyntax(base.Kind, attributeLists, awaitKeyword, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ForEachVariableStatementSyntax(base.Kind, attributeLists, awaitKeyword, forEachKeyword, openParenToken, variable, inKeyword, expression, closeParenToken, statement, GetDiagnostics(), annotations);
	}
}
