using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ScopedTypeSyntax : TypeSyntax
{
	private TypeSyntax? type;

	public SyntaxToken ScopedKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ScopedTypeSyntax)base.Green).scopedKeyword, base.Position, 0);

	public TypeSyntax Type => GetRed(ref type, 1);

	internal ScopedTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref type, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return type;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitScopedType(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitScopedType(this);
	}

	public ScopedTypeSyntax Update(SyntaxToken scopedKeyword, TypeSyntax type)
	{
		if (scopedKeyword != ScopedKeyword || type != Type)
		{
			ScopedTypeSyntax scopedTypeSyntax = SyntaxFactory.ScopedType(scopedKeyword, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return scopedTypeSyntax;
			}
			return scopedTypeSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	public ScopedTypeSyntax WithScopedKeyword(SyntaxToken scopedKeyword)
	{
		return Update(scopedKeyword, Type);
	}

	public ScopedTypeSyntax WithType(TypeSyntax type)
	{
		return Update(ScopedKeyword, type);
	}
}
