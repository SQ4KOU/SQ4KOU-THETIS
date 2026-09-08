namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AttributeSyntax : CSharpSyntaxNode
{
	internal readonly NameSyntax name;

	internal readonly AttributeArgumentListSyntax? argumentList;

	public NameSyntax Name => name;

	public AttributeArgumentListSyntax? ArgumentList => argumentList;

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax? argumentList, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		SetFlags(NodeFlags.ContainsAttributes);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax? argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SetFlags(NodeFlags.ContainsAttributes);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax? argumentList)
		: base(kind)
	{
		SetFlags(NodeFlags.ContainsAttributes);
		base.SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => name, 
			1 => argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttribute(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttribute(this);
	}

	public AttributeSyntax Update(NameSyntax name, AttributeArgumentListSyntax argumentList)
	{
		if (name != Name || argumentList != ArgumentList)
		{
			AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(name, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				attributeSyntax = attributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				attributeSyntax = attributeSyntax.WithAnnotationsGreen(annotations);
			}
			return attributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AttributeSyntax(base.Kind, name, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AttributeSyntax(base.Kind, name, argumentList, GetDiagnostics(), annotations);
	}
}
