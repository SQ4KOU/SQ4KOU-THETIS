namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class RefStructConstraintSyntax : AllowsConstraintSyntax
{
	internal readonly SyntaxToken refKeyword;

	internal readonly SyntaxToken structKeyword;

	public SyntaxToken RefKeyword => refKeyword;

	public SyntaxToken StructKeyword => structKeyword;

	internal RefStructConstraintSyntax(SyntaxKind kind, SyntaxToken refKeyword, SyntaxToken structKeyword, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(structKeyword);
		this.structKeyword = structKeyword;
	}

	internal RefStructConstraintSyntax(SyntaxKind kind, SyntaxToken refKeyword, SyntaxToken structKeyword, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(structKeyword);
		this.structKeyword = structKeyword;
	}

	internal RefStructConstraintSyntax(SyntaxKind kind, SyntaxToken refKeyword, SyntaxToken structKeyword)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(refKeyword);
		this.refKeyword = refKeyword;
		AdjustFlagsAndWidth(structKeyword);
		this.structKeyword = structKeyword;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => refKeyword, 
			1 => structKeyword, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.RefStructConstraintSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefStructConstraint(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefStructConstraint(this);
	}

	public RefStructConstraintSyntax Update(SyntaxToken refKeyword, SyntaxToken structKeyword)
	{
		if (refKeyword != RefKeyword || structKeyword != StructKeyword)
		{
			RefStructConstraintSyntax refStructConstraintSyntax = SyntaxFactory.RefStructConstraint(refKeyword, structKeyword);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				refStructConstraintSyntax = refStructConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				refStructConstraintSyntax = refStructConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return refStructConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new RefStructConstraintSyntax(base.Kind, refKeyword, structKeyword, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new RefStructConstraintSyntax(base.Kind, refKeyword, structKeyword, GetDiagnostics(), annotations);
	}
}
