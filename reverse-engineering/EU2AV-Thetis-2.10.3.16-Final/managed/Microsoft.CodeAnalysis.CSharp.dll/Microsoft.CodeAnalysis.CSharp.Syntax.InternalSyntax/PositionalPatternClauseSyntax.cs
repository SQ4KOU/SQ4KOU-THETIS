using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PositionalPatternClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode? subpatterns;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> Subpatterns => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(subpatterns));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal PositionalPatternClauseSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? subpatterns, SyntaxToken closeParenToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal PositionalPatternClauseSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? subpatterns, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal PositionalPatternClauseSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode? subpatterns, SyntaxToken closeParenToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (subpatterns != null)
		{
			AdjustFlagsAndWidth(subpatterns);
			this.subpatterns = subpatterns;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => subpatterns, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PositionalPatternClauseSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPositionalPatternClause(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPositionalPatternClause(this);
	}

	public PositionalPatternClauseSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<SubpatternSyntax> subpatterns, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || subpatterns != Subpatterns || closeParenToken != CloseParenToken)
		{
			PositionalPatternClauseSyntax positionalPatternClauseSyntax = SyntaxFactory.PositionalPatternClause(openParenToken, subpatterns, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				positionalPatternClauseSyntax = positionalPatternClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				positionalPatternClauseSyntax = positionalPatternClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return positionalPatternClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new PositionalPatternClauseSyntax(base.Kind, openParenToken, subpatterns, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new PositionalPatternClauseSyntax(base.Kind, openParenToken, subpatterns, closeParenToken, GetDiagnostics(), annotations);
	}
}
