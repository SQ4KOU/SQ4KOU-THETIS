using System;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExtensionBlockDeclarationSyntax : TypeDeclarationSyntax
{
	private SyntaxNode? attributeLists;

	private TypeParameterListSyntax? typeParameterList;

	private ParameterListSyntax? parameterList;

	private SyntaxNode? constraintClauses;

	private SyntaxNode? members;

	public override SyntaxToken Identifier => default(SyntaxToken);

	public override BaseListSyntax? BaseList => null;

	public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public override SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = base.Green.GetSlot(1);
			if (slot == null)
			{
				return default(SyntaxTokenList);
			}
			return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
		}
	}

	public override SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionBlockDeclarationSyntax)base.Green).keyword, GetChildPosition(2), GetChildIndex(2));

	public override TypeParameterListSyntax? TypeParameterList => GetRed(ref typeParameterList, 3);

	public override ParameterListSyntax? ParameterList => GetRed(ref parameterList, 4);

	public override SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new SyntaxList<TypeParameterConstraintClauseSyntax>(GetRed(ref constraintClauses, 5));

	public override SyntaxToken OpenBraceToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken openBraceToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionBlockDeclarationSyntax)base.Green).openBraceToken;
			if (openBraceToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, openBraceToken, GetChildPosition(6), GetChildIndex(6));
		}
	}

	public override SyntaxList<MemberDeclarationSyntax> Members => new SyntaxList<MemberDeclarationSyntax>(GetRed(ref members, 7));

	public override SyntaxToken CloseBraceToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken closeBraceToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionBlockDeclarationSyntax)base.Green).closeBraceToken;
			if (closeBraceToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, closeBraceToken, GetChildPosition(8), GetChildIndex(8));
		}
	}

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExtensionBlockDeclarationSyntax)base.Green).semicolonToken;
			if (semicolonToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, semicolonToken, GetChildPosition(9), GetChildIndex(9));
		}
	}

	internal override BaseTypeDeclarationSyntax WithIdentifierCore(SyntaxToken identifier)
	{
		throw new NotSupportedException();
	}

	internal override BaseTypeDeclarationSyntax AddBaseListTypesCore(params BaseTypeSyntax[] items)
	{
		throw new NotSupportedException();
	}

	internal override BaseTypeDeclarationSyntax WithBaseListCore(BaseListSyntax? baseList)
	{
		throw new NotSupportedException();
	}

	internal ExtensionBlockDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			3 => GetRed(ref typeParameterList, 3), 
			4 => GetRed(ref parameterList, 4), 
			5 => GetRed(ref constraintClauses, 5), 
			7 => GetRed(ref members, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			3 => typeParameterList, 
			4 => parameterList, 
			5 => constraintClauses, 
			7 => members, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExtensionBlockDeclaration(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitExtensionBlockDeclaration(this);
	}

	public ExtensionBlockDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || typeParameterList != TypeParameterList || parameterList != ParameterList || constraintClauses != ConstraintClauses || openBraceToken != OpenBraceToken || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			ExtensionBlockDeclarationSyntax extensionBlockDeclarationSyntax = SyntaxFactory.ExtensionBlockDeclaration(attributeLists, modifiers, keyword, typeParameterList, parameterList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return extensionBlockDeclarationSyntax;
			}
			return extensionBlockDeclarationSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override MemberDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new ExtensionBlockDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override MemberDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new ExtensionBlockDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithKeywordCore(SyntaxToken keyword)
	{
		return WithKeyword(keyword);
	}

	public new ExtensionBlockDeclarationSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(AttributeLists, Modifiers, keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithTypeParameterListCore(TypeParameterListSyntax? typeParameterList)
	{
		return WithTypeParameterList(typeParameterList);
	}

	public new ExtensionBlockDeclarationSyntax WithTypeParameterList(TypeParameterListSyntax? typeParameterList)
	{
		return Update(AttributeLists, Modifiers, Keyword, typeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithParameterListCore(ParameterListSyntax? parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new ExtensionBlockDeclarationSyntax WithParameterList(ParameterListSyntax? parameterList)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, parameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithConstraintClausesCore(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return WithConstraintClauses(constraintClauses);
	}

	public new ExtensionBlockDeclarationSyntax WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, constraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithOpenBraceTokenCore(SyntaxToken openBraceToken)
	{
		return WithOpenBraceToken(openBraceToken);
	}

	public new ExtensionBlockDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, openBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithMembersCore(SyntaxList<MemberDeclarationSyntax> members)
	{
		return WithMembers(members);
	}

	public new ExtensionBlockDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithCloseBraceTokenCore(SyntaxToken closeBraceToken)
	{
		return WithCloseBraceToken(closeBraceToken);
	}

	public new ExtensionBlockDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, closeBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new ExtensionBlockDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, TypeParameterList, ParameterList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, semicolonToken);
	}

	internal override MemberDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new ExtensionBlockDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override MemberDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new ExtensionBlockDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override TypeDeclarationSyntax AddTypeParameterListParametersCore(params TypeParameterSyntax[] items)
	{
		return AddTypeParameterListParameters(items);
	}

	public new ExtensionBlockDeclarationSyntax AddTypeParameterListParameters(params TypeParameterSyntax[] items)
	{
		TypeParameterListSyntax typeParameterListSyntax = TypeParameterList ?? SyntaxFactory.TypeParameterList();
		return WithTypeParameterList(typeParameterListSyntax.WithParameters(typeParameterListSyntax.Parameters.AddRange(items)));
	}

	internal override TypeDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new ExtensionBlockDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		ParameterListSyntax parameterListSyntax = ParameterList ?? SyntaxFactory.ParameterList();
		return WithParameterList(parameterListSyntax.WithParameters(parameterListSyntax.Parameters.AddRange(items)));
	}

	internal override TypeDeclarationSyntax AddConstraintClausesCore(params TypeParameterConstraintClauseSyntax[] items)
	{
		return AddConstraintClauses(items);
	}

	public new ExtensionBlockDeclarationSyntax AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
	{
		return WithConstraintClauses(ConstraintClauses.AddRange(items));
	}

	internal override TypeDeclarationSyntax AddMembersCore(params MemberDeclarationSyntax[] items)
	{
		return AddMembers(items);
	}

	public new ExtensionBlockDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
