namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlCrefAttributeSyntax : XmlAttributeSyntax
{
	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken equalsToken;

	internal readonly SyntaxToken startQuoteToken;

	internal readonly CrefSyntax cref;

	internal readonly SyntaxToken endQuoteToken;

	public override XmlNameSyntax Name => name;

	public override SyntaxToken EqualsToken => equalsToken;

	public override SyntaxToken StartQuoteToken => startQuoteToken;

	public CrefSyntax Cref => cref;

	public override SyntaxToken EndQuoteToken => endQuoteToken;

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal XmlCrefAttributeSyntax(SyntaxKind kind, XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
		: base(kind)
	{
		base.SlotCount = 5;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
		AdjustFlagsAndWidth(startQuoteToken);
		this.startQuoteToken = startQuoteToken;
		AdjustFlagsAndWidth(cref);
		this.cref = cref;
		AdjustFlagsAndWidth(endQuoteToken);
		this.endQuoteToken = endQuoteToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => name, 
			1 => equalsToken, 
			2 => startQuoteToken, 
			3 => cref, 
			4 => endQuoteToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlCrefAttributeSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlCrefAttribute(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlCrefAttribute(this);
	}

	public XmlCrefAttributeSyntax Update(XmlNameSyntax name, SyntaxToken equalsToken, SyntaxToken startQuoteToken, CrefSyntax cref, SyntaxToken endQuoteToken)
	{
		if (name != Name || equalsToken != EqualsToken || startQuoteToken != StartQuoteToken || cref != Cref || endQuoteToken != EndQuoteToken)
		{
			XmlCrefAttributeSyntax xmlCrefAttributeSyntax = SyntaxFactory.XmlCrefAttribute(name, equalsToken, startQuoteToken, cref, endQuoteToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlCrefAttributeSyntax = xmlCrefAttributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlCrefAttributeSyntax = xmlCrefAttributeSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlCrefAttributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlCrefAttributeSyntax(base.Kind, name, equalsToken, startQuoteToken, cref, endQuoteToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlCrefAttributeSyntax(base.Kind, name, equalsToken, startQuoteToken, cref, endQuoteToken, GetDiagnostics(), annotations);
	}
}
