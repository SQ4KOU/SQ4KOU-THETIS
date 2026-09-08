namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlElementEndTagSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanSlashToken;

	internal readonly XmlNameSyntax name;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanSlashToken => lessThanSlashToken;

	public XmlNameSyntax Name => name;

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal XmlElementEndTagSyntax(SyntaxKind kind, SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(lessThanSlashToken);
		this.lessThanSlashToken = lessThanSlashToken;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanSlashToken, 
			1 => name, 
			2 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElementEndTag(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElementEndTag(this);
	}

	public XmlElementEndTagSyntax Update(SyntaxToken lessThanSlashToken, XmlNameSyntax name, SyntaxToken greaterThanToken)
	{
		if (lessThanSlashToken != LessThanSlashToken || name != Name || greaterThanToken != GreaterThanToken)
		{
			XmlElementEndTagSyntax xmlElementEndTagSyntax = SyntaxFactory.XmlElementEndTag(lessThanSlashToken, name, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlElementEndTagSyntax = xmlElementEndTagSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlElementEndTagSyntax = xmlElementEndTagSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlElementEndTagSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlElementEndTagSyntax(base.Kind, lessThanSlashToken, name, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlElementEndTagSyntax(base.Kind, lessThanSlashToken, name, greaterThanToken, GetDiagnostics(), annotations);
	}
}
