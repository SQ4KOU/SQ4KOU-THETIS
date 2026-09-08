using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlEmptyElementSyntax : XmlNodeSyntax
{
	internal readonly SyntaxToken lessThanToken;

	internal readonly XmlNameSyntax name;

	internal readonly GreenNode? attributes;

	internal readonly SyntaxToken slashGreaterThanToken;

	public SyntaxToken LessThanToken => lessThanToken;

	public XmlNameSyntax Name => name;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> Attributes => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax>(attributes);

	public SyntaxToken SlashGreaterThanToken => slashGreaterThanToken;

	internal XmlEmptyElementSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode? attributes, SyntaxToken slashGreaterThanToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(slashGreaterThanToken);
		this.slashGreaterThanToken = slashGreaterThanToken;
	}

	internal XmlEmptyElementSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode? attributes, SyntaxToken slashGreaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(slashGreaterThanToken);
		this.slashGreaterThanToken = slashGreaterThanToken;
	}

	internal XmlEmptyElementSyntax(SyntaxKind kind, SyntaxToken lessThanToken, XmlNameSyntax name, GreenNode? attributes, SyntaxToken slashGreaterThanToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(slashGreaterThanToken);
		this.slashGreaterThanToken = slashGreaterThanToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanToken, 
			1 => name, 
			2 => attributes, 
			3 => slashGreaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlEmptyElementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlEmptyElement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlEmptyElement(this);
	}

	public XmlEmptyElementSyntax Update(SyntaxToken lessThanToken, XmlNameSyntax name, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlAttributeSyntax> attributes, SyntaxToken slashGreaterThanToken)
	{
		if (lessThanToken != LessThanToken || name != Name || attributes != Attributes || slashGreaterThanToken != SlashGreaterThanToken)
		{
			XmlEmptyElementSyntax xmlEmptyElementSyntax = SyntaxFactory.XmlEmptyElement(lessThanToken, name, attributes, slashGreaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlEmptyElementSyntax = xmlEmptyElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlEmptyElementSyntax = xmlEmptyElementSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlEmptyElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlEmptyElementSyntax(base.Kind, lessThanToken, name, attributes, slashGreaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlEmptyElementSyntax(base.Kind, lessThanToken, name, attributes, slashGreaterThanToken, GetDiagnostics(), annotations);
	}
}
