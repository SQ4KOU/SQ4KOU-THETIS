namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypePatternSyntax : PatternSyntax
{
	internal readonly TypeSyntax type;

	public TypeSyntax Type => type;

	internal TypePatternSyntax(SyntaxKind kind, TypeSyntax type, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypePatternSyntax(SyntaxKind kind, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypePatternSyntax(SyntaxKind kind, TypeSyntax type)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypePatternSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypePattern(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypePattern(this);
	}

	public TypePatternSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypePatternSyntax typePatternSyntax = SyntaxFactory.TypePattern(type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typePatternSyntax = typePatternSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typePatternSyntax = typePatternSyntax.WithAnnotationsGreen(annotations);
			}
			return typePatternSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TypePatternSyntax(base.Kind, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TypePatternSyntax(base.Kind, type, GetDiagnostics(), annotations);
	}
}
