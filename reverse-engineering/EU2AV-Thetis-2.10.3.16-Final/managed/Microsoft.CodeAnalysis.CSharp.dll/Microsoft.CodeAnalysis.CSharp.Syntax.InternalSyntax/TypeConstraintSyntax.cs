namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly TypeSyntax type;

	public TypeSyntax Type => type;

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type)
		: base(kind)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal override GreenNode? GetSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return type;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeConstraintSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeConstraint(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeConstraint(this);
	}

	public TypeConstraintSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypeConstraintSyntax typeConstraintSyntax = SyntaxFactory.TypeConstraint(type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeConstraintSyntax = typeConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeConstraintSyntax = typeConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return typeConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TypeConstraintSyntax(base.Kind, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TypeConstraintSyntax(base.Kind, type, GetDiagnostics(), annotations);
	}
}
