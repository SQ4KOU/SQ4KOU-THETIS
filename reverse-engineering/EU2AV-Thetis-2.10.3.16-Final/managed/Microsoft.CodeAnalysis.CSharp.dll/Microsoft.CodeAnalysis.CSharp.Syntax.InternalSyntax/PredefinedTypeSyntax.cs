namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PredefinedTypeSyntax : TypeSyntax
{
	internal readonly SyntaxToken keyword;

	public SyntaxToken Keyword => keyword;

	internal PredefinedTypeSyntax(SyntaxKind kind, SyntaxToken keyword, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
	}

	internal PredefinedTypeSyntax(SyntaxKind kind, SyntaxToken keyword, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
	}

	internal PredefinedTypeSyntax(SyntaxKind kind, SyntaxToken keyword)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return keyword;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PredefinedTypeSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPredefinedType(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPredefinedType(this);
	}

	public PredefinedTypeSyntax Update(SyntaxToken keyword)
	{
		if (keyword != Keyword)
		{
			PredefinedTypeSyntax predefinedTypeSyntax = SyntaxFactory.PredefinedType(keyword);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				predefinedTypeSyntax = predefinedTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				predefinedTypeSyntax = predefinedTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return predefinedTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new PredefinedTypeSyntax(base.Kind, keyword, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new PredefinedTypeSyntax(base.Kind, keyword, GetDiagnostics(), annotations);
	}
}
