namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlNameSyntax : CSharpSyntaxNode
{
	internal readonly XmlPrefixSyntax? prefix;

	internal readonly SyntaxToken localName;

	public XmlPrefixSyntax? Prefix => prefix;

	public SyntaxToken LocalName => localName;

	internal XmlNameSyntax(SyntaxKind kind, XmlPrefixSyntax? prefix, SyntaxToken localName, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 2;
		if (prefix != null)
		{
			AdjustFlagsAndWidth(prefix);
			this.prefix = prefix;
		}
		AdjustFlagsAndWidth(localName);
		this.localName = localName;
	}

	internal XmlNameSyntax(SyntaxKind kind, XmlPrefixSyntax? prefix, SyntaxToken localName, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 2;
		if (prefix != null)
		{
			AdjustFlagsAndWidth(prefix);
			this.prefix = prefix;
		}
		AdjustFlagsAndWidth(localName);
		this.localName = localName;
	}

	internal XmlNameSyntax(SyntaxKind kind, XmlPrefixSyntax? prefix, SyntaxToken localName)
		: base(kind)
	{
		base.SlotCount = 2;
		if (prefix != null)
		{
			AdjustFlagsAndWidth(prefix);
			this.prefix = prefix;
		}
		AdjustFlagsAndWidth(localName);
		this.localName = localName;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => prefix, 
			1 => localName, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlName(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlName(this);
	}

	public XmlNameSyntax Update(XmlPrefixSyntax prefix, SyntaxToken localName)
	{
		if (prefix != Prefix || localName != LocalName)
		{
			XmlNameSyntax xmlNameSyntax = SyntaxFactory.XmlName(prefix, localName);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlNameSyntax = xmlNameSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlNameSyntax = xmlNameSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlNameSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlNameSyntax(base.Kind, prefix, localName, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlNameSyntax(base.Kind, prefix, localName, GetDiagnostics(), annotations);
	}
}
