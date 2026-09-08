using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AttributeListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly AttributeTargetSpecifierSyntax? target;

	internal readonly GreenNode? attributes;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public AttributeTargetSpecifierSyntax? Target => target;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> Attributes => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(attributes));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal AttributeListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax? target, GreenNode? attributes, SyntaxToken closeBracketToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (target != null)
		{
			AdjustFlagsAndWidth(target);
			this.target = target;
		}
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal AttributeListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax? target, GreenNode? attributes, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (target != null)
		{
			AdjustFlagsAndWidth(target);
			this.target = target;
		}
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal AttributeListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax? target, GreenNode? attributes, SyntaxToken closeBracketToken)
		: base(kind)
	{
		base.SlotCount = 4;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (target != null)
		{
			AdjustFlagsAndWidth(target);
			this.target = target;
		}
		if (attributes != null)
		{
			AdjustFlagsAndWidth(attributes);
			this.attributes = attributes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => target, 
			2 => attributes, 
			3 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttributeList(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeList(this);
	}

	public AttributeListSyntax Update(SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax target, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AttributeSyntax> attributes, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || target != Target || attributes != Attributes || closeBracketToken != CloseBracketToken)
		{
			AttributeListSyntax attributeListSyntax = SyntaxFactory.AttributeList(openBracketToken, target, attributes, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				attributeListSyntax = attributeListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				attributeListSyntax = attributeListSyntax.WithAnnotationsGreen(annotations);
			}
			return attributeListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new AttributeListSyntax(base.Kind, openBracketToken, target, attributes, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new AttributeListSyntax(base.Kind, openBracketToken, target, attributes, closeBracketToken, GetDiagnostics(), annotations);
	}
}
