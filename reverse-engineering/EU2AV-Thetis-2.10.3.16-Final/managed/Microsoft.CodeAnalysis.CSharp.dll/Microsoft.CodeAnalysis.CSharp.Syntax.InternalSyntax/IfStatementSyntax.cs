using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IfStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken ifKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken closeParenToken;

	internal readonly StatementSyntax statement;

	internal readonly ElseClauseSyntax? @else;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken IfKeyword => ifKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Condition => condition;

	public SyntaxToken CloseParenToken => closeParenToken;

	public StatementSyntax Statement => statement;

	public ElseClauseSyntax? Else => @else;

	internal IfStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal IfStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal IfStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax? @else)
		: base(kind)
	{
		base.SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
		if (@else != null)
		{
			AdjustFlagsAndWidth(@else);
			this.@else = @else;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => ifKeyword, 
			2 => openParenToken, 
			3 => condition, 
			4 => closeParenToken, 
			5 => statement, 
			6 => @else, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public IfStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken ifKeyword, SyntaxToken openParenToken, ExpressionSyntax condition, SyntaxToken closeParenToken, StatementSyntax statement, ElseClauseSyntax @else)
	{
		if (attributeLists != AttributeLists || ifKeyword != IfKeyword || openParenToken != OpenParenToken || condition != Condition || closeParenToken != CloseParenToken || statement != Statement || @else != Else)
		{
			IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(attributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				ifStatementSyntax = ifStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				ifStatementSyntax = ifStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return ifStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new IfStatementSyntax(base.Kind, attributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new IfStatementSyntax(base.Kind, attributeLists, ifKeyword, openParenToken, condition, closeParenToken, statement, @else, GetDiagnostics(), annotations);
	}
}
