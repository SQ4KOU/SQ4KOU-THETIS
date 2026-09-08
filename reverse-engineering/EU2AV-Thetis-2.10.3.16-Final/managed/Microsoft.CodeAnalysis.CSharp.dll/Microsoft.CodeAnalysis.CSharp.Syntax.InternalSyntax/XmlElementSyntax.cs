using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class XmlElementSyntax : XmlNodeSyntax
{
	internal readonly XmlElementStartTagSyntax startTag;

	internal readonly GreenNode? content;

	internal readonly XmlElementEndTagSyntax endTag;

	public XmlElementStartTagSyntax StartTag => startTag;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> Content => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax>(content);

	public XmlElementEndTagSyntax EndTag => endTag;

	internal XmlElementSyntax(SyntaxKind kind, XmlElementStartTagSyntax startTag, GreenNode? content, XmlElementEndTagSyntax endTag, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(startTag);
		this.startTag = startTag;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endTag);
		this.endTag = endTag;
	}

	internal XmlElementSyntax(SyntaxKind kind, XmlElementStartTagSyntax startTag, GreenNode? content, XmlElementEndTagSyntax endTag, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 3;
		AdjustFlagsAndWidth(startTag);
		this.startTag = startTag;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endTag);
		this.endTag = endTag;
	}

	internal XmlElementSyntax(SyntaxKind kind, XmlElementStartTagSyntax startTag, GreenNode? content, XmlElementEndTagSyntax endTag)
		: base(kind)
	{
		base.SlotCount = 3;
		AdjustFlagsAndWidth(startTag);
		this.startTag = startTag;
		if (content != null)
		{
			AdjustFlagsAndWidth(content);
			this.content = content;
		}
		AdjustFlagsAndWidth(endTag);
		this.endTag = endTag;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => startTag, 
			1 => content, 
			2 => endTag, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitXmlElement(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlElement(this);
	}

	public XmlElementSyntax Update(XmlElementStartTagSyntax startTag, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<XmlNodeSyntax> content, XmlElementEndTagSyntax endTag)
	{
		if (startTag != StartTag || content != Content || endTag != EndTag)
		{
			XmlElementSyntax xmlElementSyntax = SyntaxFactory.XmlElement(startTag, content, endTag);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				xmlElementSyntax = xmlElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				xmlElementSyntax = xmlElementSyntax.WithAnnotationsGreen(annotations);
			}
			return xmlElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new XmlElementSyntax(base.Kind, startTag, content, endTag, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new XmlElementSyntax(base.Kind, startTag, content, endTag, GetDiagnostics(), annotations);
	}
}
