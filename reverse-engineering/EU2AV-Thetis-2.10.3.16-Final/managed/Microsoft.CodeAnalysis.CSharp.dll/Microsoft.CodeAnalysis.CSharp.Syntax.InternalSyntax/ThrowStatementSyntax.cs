using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ThrowStatementSyntax : StatementSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly SyntaxToken throwKeyword;

	internal readonly ExpressionSyntax? expression;

	internal readonly SyntaxToken semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken ThrowKeyword => throwKeyword;

	public ExpressionSyntax? Expression => expression;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ThrowStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken throwKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ThrowStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken throwKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
		if (expression != null)
		{
			AdjustFlagsAndWidth(expression);
			this.expression = expression;
		}
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ThrowStatementSyntax(SyntaxKind kind, GreenNode? attributeLists, SyntaxToken throwKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
		: base(kind)
	{
		base.SlotCount = 4;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(throwKeyword);
		this.throwKeyword = throwKeyword;
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
			1 => throwKeyword, 
			2 => expression, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThrowStatement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitThrowStatement(this);
	}

	public ThrowStatementSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken throwKeyword, ExpressionSyntax expression, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || throwKeyword != ThrowKeyword || expression != Expression || semicolonToken != SemicolonToken)
		{
			ThrowStatementSyntax throwStatementSyntax = SyntaxFactory.ThrowStatement(attributeLists, throwKeyword, expression, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				throwStatementSyntax = throwStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				throwStatementSyntax = throwStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return throwStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ThrowStatementSyntax(base.Kind, attributeLists, throwKeyword, expression, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ThrowStatementSyntax(base.Kind, attributeLists, throwKeyword, expression, semicolonToken, GetDiagnostics(), annotations);
	}
}
