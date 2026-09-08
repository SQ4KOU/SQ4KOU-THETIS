namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TupleElementSyntax : CSharpSyntaxNode
{
	internal readonly TypeSyntax type;

	internal readonly SyntaxToken? identifier;

	public TypeSyntax Type => type;

	public SyntaxToken? Identifier => identifier;

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken? identifier, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken? identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken? identifier)
		: base(kind)
	{
		base.SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => type, 
			1 => identifier, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTupleElement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTupleElement(this);
	}

	public TupleElementSyntax Update(TypeSyntax type, SyntaxToken identifier)
	{
		if (type != Type || identifier != Identifier)
		{
			TupleElementSyntax tupleElementSyntax = SyntaxFactory.TupleElement(type, identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				tupleElementSyntax = tupleElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				tupleElementSyntax = tupleElementSyntax.WithAnnotationsGreen(annotations);
			}
			return tupleElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new TupleElementSyntax(base.Kind, type, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new TupleElementSyntax(base.Kind, type, identifier, GetDiagnostics(), annotations);
	}
}
