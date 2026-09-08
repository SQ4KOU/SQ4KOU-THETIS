using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PropertyPatternClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode? subpatterns;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> Subpatterns => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(subpatterns));

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal PropertyPatternClauseSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? subpatterns, SyntaxToken closeBraceToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal PropertyPatternClauseSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? subpatterns, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal PropertyPatternClauseSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode? subpatterns, SyntaxToken closeBraceToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => subpatterns, 
			2 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PropertyPatternClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPropertyPatternClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPropertyPatternClause(this);
	}

	public PropertyPatternClauseSyntax Update(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> subpatterns, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || subpatterns != Subpatterns || closeBraceToken != CloseBraceToken)
		{
			PropertyPatternClauseSyntax propertyPatternClauseSyntax = SyntaxFactory.PropertyPatternClause(openBraceToken, subpatterns, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				propertyPatternClauseSyntax = propertyPatternClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				propertyPatternClauseSyntax = propertyPatternClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return propertyPatternClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new PropertyPatternClauseSyntax(base.Kind, openBraceToken, subpatterns, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new PropertyPatternClauseSyntax(base.Kind, openBraceToken, subpatterns, closeBraceToken, GetDiagnostics(), annotations);
	}
}
