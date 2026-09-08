using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class GenericNameSyntax : SimpleNameSyntax
{
	private TypeArgumentListSyntax? typeArgumentList;

	public bool IsUnboundGenericName => TypeArgumentList.Arguments.Any(SyntaxKind.OmittedTypeArgument);

	public override SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.GenericNameSyntax)base.Green).identifier, base.Position, 0);

	public TypeArgumentListSyntax TypeArgumentList => GetRed(ref typeArgumentList, 1);

	internal override string ErrorDisplayName()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		instance.Builder.Append(Identifier.ValueText).Append('<').Append(',', base.Arity - 1)
			.Append('>');
		return instance.ToStringAndFree();
	}

	internal GenericNameSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return GetRed(ref typeArgumentList, 1);
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		if (index != 1)
		{
			return null;
		}
		return typeArgumentList;
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGenericName(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitGenericName(this);
	}

	public GenericNameSyntax Update(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
	{
		if (identifier != Identifier || typeArgumentList != TypeArgumentList)
		{
			GenericNameSyntax genericNameSyntax = SyntaxFactory.GenericName(identifier, typeArgumentList);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return genericNameSyntax;
			}
			return genericNameSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override SimpleNameSyntax WithIdentifierCore(SyntaxToken identifier)
	{
		return WithIdentifier(identifier);
	}

	public new GenericNameSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier, TypeArgumentList);
	}

	public GenericNameSyntax WithTypeArgumentList(TypeArgumentListSyntax typeArgumentList)
	{
		return Update(Identifier, typeArgumentList);
	}

	public GenericNameSyntax AddTypeArgumentListArguments(params TypeSyntax[] items)
	{
		return WithTypeArgumentList(TypeArgumentList.WithArguments(TypeArgumentList.Arguments.AddRange(items)));
	}
}
