using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SimpleBaseTypeSyntax : BaseTypeSyntax
{
	private TypeSyntax? type;

	public override TypeSyntax Type => GetRedAtZero(ref type);

	internal SimpleBaseTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return GetRedAtZero(ref type);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 0)
		{
			return null;
		}
		return type;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleBaseType(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitSimpleBaseType(this);
	}

	public SimpleBaseTypeSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			SimpleBaseTypeSyntax simpleBaseTypeSyntax = SyntaxFactory.SimpleBaseType(type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return simpleBaseTypeSyntax;
			}
			return simpleBaseTypeSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override BaseTypeSyntax WithTypeCore(TypeSyntax type)
	{
		return WithType(type);
	}

	public new SimpleBaseTypeSyntax WithType(TypeSyntax type)
	{
		return Update(type);
	}
}
