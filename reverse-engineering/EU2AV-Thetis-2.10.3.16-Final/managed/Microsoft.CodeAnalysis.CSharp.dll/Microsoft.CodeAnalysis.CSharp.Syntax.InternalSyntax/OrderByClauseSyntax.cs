using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OrderByClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken orderByKeyword;

	internal readonly GreenNode? orderings;

	public SyntaxToken OrderByKeyword => orderByKeyword;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax> Orderings => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(orderings));

	internal OrderByClauseSyntax(SyntaxKind kind, SyntaxToken orderByKeyword, GreenNode? orderings, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(orderByKeyword);
		this.orderByKeyword = orderByKeyword;
		if (orderings != null)
		{
			AdjustFlagsAndWidth(orderings);
			this.orderings = orderings;
		}
	}

	internal OrderByClauseSyntax(SyntaxKind kind, SyntaxToken orderByKeyword, GreenNode? orderings, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(orderByKeyword);
		this.orderByKeyword = orderByKeyword;
		if (orderings != null)
		{
			AdjustFlagsAndWidth(orderings);
			this.orderings = orderings;
		}
	}

	internal OrderByClauseSyntax(SyntaxKind kind, SyntaxToken orderByKeyword, GreenNode? orderings)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(orderByKeyword);
		this.orderByKeyword = orderByKeyword;
		if (orderings != null)
		{
			AdjustFlagsAndWidth(orderings);
			this.orderings = orderings;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => orderByKeyword, 
			1 => orderings, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OrderByClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOrderByClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByClause(this);
	}

	public OrderByClauseSyntax Update(SyntaxToken orderByKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<OrderingSyntax> orderings)
	{
		if (orderByKeyword != OrderByKeyword || orderings != Orderings)
		{
			OrderByClauseSyntax orderByClauseSyntax = SyntaxFactory.OrderByClause(orderByKeyword, orderings);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				orderByClauseSyntax = orderByClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				orderByClauseSyntax = orderByClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return orderByClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new OrderByClauseSyntax(base.Kind, orderByKeyword, orderings, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new OrderByClauseSyntax(base.Kind, orderByKeyword, orderings, GetDiagnostics(), annotations);
	}
}
