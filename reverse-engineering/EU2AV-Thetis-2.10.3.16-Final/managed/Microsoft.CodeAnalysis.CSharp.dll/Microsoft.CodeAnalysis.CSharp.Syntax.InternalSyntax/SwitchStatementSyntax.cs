using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SwitchStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken switchKeyword;

	internal readonly SyntaxToken? openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken? closeParenToken;

	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode? sections;

	internal readonly SyntaxToken closeBraceToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken SwitchKeyword => switchKeyword;

	public SyntaxToken? OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken? CloseParenToken => closeParenToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> Sections => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax>(sections);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal SwitchStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken switchKeyword, SyntaxToken? openParenToken, ExpressionSyntax expression, SyntaxToken? closeParenToken, SyntaxToken openBraceToken, GreenNode? sections, SyntaxToken closeBraceToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		if (openParenToken != null)
		{
			AdjustFlagsAndWidth(openParenToken);
			this.openParenToken = openParenToken;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (closeParenToken != null)
		{
			AdjustFlagsAndWidth(closeParenToken);
			this.closeParenToken = closeParenToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal SwitchStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken switchKeyword, SyntaxToken? openParenToken, ExpressionSyntax expression, SyntaxToken? closeParenToken, SyntaxToken openBraceToken, GreenNode? sections, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		if (openParenToken != null)
		{
			AdjustFlagsAndWidth(openParenToken);
			this.openParenToken = openParenToken;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (closeParenToken != null)
		{
			AdjustFlagsAndWidth(closeParenToken);
			this.closeParenToken = closeParenToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal SwitchStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken switchKeyword, SyntaxToken? openParenToken, ExpressionSyntax expression, SyntaxToken? closeParenToken, SyntaxToken openBraceToken, GreenNode? sections, SyntaxToken closeBraceToken)
		: base(kind)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		if (openParenToken != null)
		{
			AdjustFlagsAndWidth(openParenToken);
			this.openParenToken = openParenToken;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (closeParenToken != null)
		{
			AdjustFlagsAndWidth(closeParenToken);
			this.closeParenToken = closeParenToken;
		}
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => switchKeyword, 
			2 => openParenToken, 
			3 => expression, 
			4 => closeParenToken, 
			5 => openBraceToken, 
			6 => sections, 
			7 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSwitchStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSwitchStatement(this);
	}

	public SwitchStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		if (attributeLists != AttributeLists || switchKeyword != SwitchKeyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken || openBraceToken != OpenBraceToken || sections != Sections || closeBraceToken != CloseBraceToken)
		{
			SwitchStatementSyntax switchStatementSyntax = SyntaxFactory.SwitchStatement(attributeLists, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				switchStatementSyntax = switchStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				switchStatementSyntax = switchStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return switchStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SwitchStatementSyntax(base.Kind, attributeLists, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SwitchStatementSyntax(base.Kind, attributeLists, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken, GetDiagnostics(), annotations);
	}
}
