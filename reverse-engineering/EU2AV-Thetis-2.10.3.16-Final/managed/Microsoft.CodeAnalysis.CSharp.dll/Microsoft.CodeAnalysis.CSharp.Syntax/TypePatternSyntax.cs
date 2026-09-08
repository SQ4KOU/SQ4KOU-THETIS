using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TypePatternSyntax : PatternSyntax
{
	private TypeSyntax? type;

	public TypeSyntax Type => GetRedAtZero(ref type);

	internal TypePatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
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
		visitor.VisitTypePattern(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitTypePattern(this);
	}

	public TypePatternSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypePatternSyntax typePatternSyntax = SyntaxFactory.TypePattern(type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return typePatternSyntax;
			}
			return typePatternSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public TypePatternSyntax WithType(TypeSyntax type)
	{
		return Update(type);
	}
}
