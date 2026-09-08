using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CollectionExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode? elements;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CollectionElementSyntax> Elements => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CollectionElementSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(elements));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal CollectionExpressionSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? elements, SyntaxToken closeBracketToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal CollectionExpressionSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? elements, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal CollectionExpressionSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode? elements, SyntaxToken closeBracketToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => elements, 
			2 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CollectionExpressionSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCollectionExpression(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCollectionExpression(this);
	}

	public CollectionExpressionSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CollectionElementSyntax> elements, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || elements != Elements || closeBracketToken != CloseBracketToken)
		{
			CollectionExpressionSyntax collectionExpressionSyntax = SyntaxFactory.CollectionExpression(openBracketToken, elements, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				collectionExpressionSyntax = collectionExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				collectionExpressionSyntax = collectionExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return collectionExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new CollectionExpressionSyntax(base.Kind, openBracketToken, elements, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new CollectionExpressionSyntax(base.Kind, openBracketToken, elements, closeBracketToken, GetDiagnostics(), annotations);
	}
}
