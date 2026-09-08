using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ReturnStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken returnKeyword;

	internal readonly ExpressionSyntax? expression;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken ReturnKeyword => returnKeyword;

	public ExpressionSyntax? Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ReturnStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ReturnStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ReturnStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(returnKeyword);
		this.returnKeyword = returnKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => returnKeyword, 
			2 => expression, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitReturnStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReturnStatement(this);
	}

	public ReturnStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken returnKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || returnKeyword != ReturnKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ReturnStatementSyntax returnStatementSyntax = SyntaxFactory.ReturnStatement(attributeLists, returnKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				returnStatementSyntax = returnStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				returnStatementSyntax = returnStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return returnStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ReturnStatementSyntax(base.Kind, attributeLists, returnKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ReturnStatementSyntax(base.Kind, attributeLists, returnKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}
}
