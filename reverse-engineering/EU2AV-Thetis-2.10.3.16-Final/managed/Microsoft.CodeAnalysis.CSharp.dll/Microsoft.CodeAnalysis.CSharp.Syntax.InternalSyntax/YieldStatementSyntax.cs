using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class YieldStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken yieldKeyword;

	internal readonly SyntaxToken returnOrBreakKeyword;

	internal readonly ExpressionSyntax? expression;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken YieldKeyword => yieldKeyword;

	public SyntaxToken ReturnOrBreakKeyword => returnOrBreakKeyword;

	public ExpressionSyntax? Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal YieldStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal YieldStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal YieldStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 5;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(yieldKeyword);
		this.yieldKeyword = yieldKeyword;
		AdjustFlagsAndWidth(returnOrBreakKeyword);
		this.returnOrBreakKeyword = returnOrBreakKeyword;
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
			1 => yieldKeyword, 
			2 => returnOrBreakKeyword, 
			3 => expression, 
			4 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitYieldStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitYieldStatement(this);
	}

	public YieldStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken yieldKeyword, SyntaxToken returnOrBreakKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || yieldKeyword != YieldKeyword || returnOrBreakKeyword != ReturnOrBreakKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			YieldStatementSyntax yieldStatementSyntax = SyntaxFactory.YieldStatement(base.Kind, attributeLists, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				yieldStatementSyntax = yieldStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				yieldStatementSyntax = yieldStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return yieldStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new YieldStatementSyntax(base.Kind, attributeLists, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new YieldStatementSyntax(base.Kind, attributeLists, yieldKeyword, returnOrBreakKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}
}
