namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ScopedTypeSyntax : TypeSyntax
{
	internal readonly SyntaxToken scopedKeyword;

	internal readonly TypeSyntax type;

	public SyntaxToken ScopedKeyword => scopedKeyword;

	public TypeSyntax Type => type;

	internal ScopedTypeSyntax(SyntaxKind kind, SyntaxToken scopedKeyword, TypeSyntax type, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(scopedKeyword);
		this.scopedKeyword = scopedKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal ScopedTypeSyntax(SyntaxKind kind, SyntaxToken scopedKeyword, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(scopedKeyword);
		this.scopedKeyword = scopedKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal ScopedTypeSyntax(SyntaxKind kind, SyntaxToken scopedKeyword, TypeSyntax type)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(scopedKeyword);
		this.scopedKeyword = scopedKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => scopedKeyword, 
			1 => type, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ScopedTypeSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitScopedType(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitScopedType(this);
	}

	public ScopedTypeSyntax Update(SyntaxToken scopedKeyword, TypeSyntax type)
	{
		if (scopedKeyword != ScopedKeyword || type != Type)
		{
			ScopedTypeSyntax scopedTypeSyntax = SyntaxFactory.ScopedType(scopedKeyword, type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				scopedTypeSyntax = scopedTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				scopedTypeSyntax = scopedTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return scopedTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new ScopedTypeSyntax(base.Kind, scopedKeyword, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new ScopedTypeSyntax(base.Kind, scopedKeyword, type, GetDiagnostics(), annotations);
	}
}
