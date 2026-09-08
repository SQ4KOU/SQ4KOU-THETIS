using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ClassDeclarationSyntax : TypeDeclarationSyntax
{
	private SyntaxNode? attributeLists;

	private TypeParameterListSyntax? typeParameterList;

	private ParameterListSyntax? parameterList;

	private BaseListSyntax? baseList;

	private SyntaxNode? constraintClauses;

	private SyntaxNode? members;

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

	public override SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassDeclarationSyntax)base.Green).keyword, GetChildPosition(2), GetChildIndex(2));

	public override SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassDeclarationSyntax)base.Green).identifier, GetChildPosition(3), GetChildIndex(3));

	public override TypeParameterListSyntax? TypeParameterList => GetRed(ref typeParameterList, 4);

	public override ParameterListSyntax? ParameterList => GetRed(ref parameterList, 5);

	public override BaseListSyntax? BaseList => GetRed(ref baseList, 6);

	public override SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses => new SyntaxList<TypeParameterConstraintClauseSyntax>(GetRed(ref constraintClauses, 7));

	public override SyntaxToken OpenBraceToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken openBraceToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassDeclarationSyntax)base.Green).openBraceToken;
			if (openBraceToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, openBraceToken, GetChildPosition(8), GetChildIndex(8));
		}
	}

	public override SyntaxList<MemberDeclarationSyntax> Members => new SyntaxList<MemberDeclarationSyntax>(GetRed(ref members, 9));

	public override SyntaxToken CloseBraceToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken closeBraceToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassDeclarationSyntax)base.Green).closeBraceToken;
			if (closeBraceToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, closeBraceToken, GetChildPosition(10), GetChildIndex(10));
		}
	}

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassDeclarationSyntax)base.Green).semicolonToken;
			if (semicolonToken == null)
			{
				return default(SyntaxToken);
			}
			return new SyntaxToken(this, semicolonToken, GetChildPosition(11), GetChildIndex(11));
		}
	}

	public ClassDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, BaseListSyntax? baseList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		return Update(attributeLists, modifiers, keyword, identifier, typeParameterList, ParameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
	}

	internal ClassDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			4 => GetRed(ref typeParameterList, 4), 
			5 => GetRed(ref parameterList, 5), 
			6 => GetRed(ref baseList, 6), 
			7 => GetRed(ref constraintClauses, 7), 
			9 => GetRed(ref members, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			4 => typeParameterList, 
			5 => parameterList, 
			6 => baseList, 
			7 => constraintClauses, 
			9 => members, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitClassDeclaration(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitClassDeclaration(this);
	}

	public ClassDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken keyword, SyntaxToken identifier, TypeParameterListSyntax? typeParameterList, ParameterListSyntax? parameterList, BaseListSyntax? baseList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, SyntaxToken openBraceToken, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || keyword != Keyword || identifier != Identifier || typeParameterList != TypeParameterList || parameterList != ParameterList || baseList != BaseList || constraintClauses != ConstraintClauses || openBraceToken != OpenBraceToken || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			ClassDeclarationSyntax classDeclarationSyntax = SyntaxFactory.ClassDeclaration(attributeLists, modifiers, keyword, identifier, typeParameterList, parameterList, baseList, constraintClauses, openBraceToken, members, closeBraceToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return classDeclarationSyntax;
			}
			return classDeclarationSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override MemberDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new ClassDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override MemberDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new ClassDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithKeywordCore(SyntaxToken keyword)
	{
		return WithKeyword(keyword);
	}

	public new ClassDeclarationSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(AttributeLists, Modifiers, keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithIdentifierCore(SyntaxToken identifier)
	{
		return WithIdentifier(identifier);
	}

	public new ClassDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, Keyword, identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithTypeParameterListCore(TypeParameterListSyntax? typeParameterList)
	{
		return WithTypeParameterList(typeParameterList);
	}

	public new ClassDeclarationSyntax WithTypeParameterList(TypeParameterListSyntax? typeParameterList)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, typeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithParameterListCore(ParameterListSyntax? parameterList)
	{
		return WithParameterList(parameterList);
	}

	public new ClassDeclarationSyntax WithParameterList(ParameterListSyntax? parameterList)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, parameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithBaseListCore(BaseListSyntax? baseList)
	{
		return WithBaseList(baseList);
	}

	public new ClassDeclarationSyntax WithBaseList(BaseListSyntax? baseList)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, baseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithConstraintClausesCore(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return WithConstraintClauses(constraintClauses);
	}

	public new ClassDeclarationSyntax WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, constraintClauses, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithOpenBraceTokenCore(SyntaxToken openBraceToken)
	{
		return WithOpenBraceToken(openBraceToken);
	}

	public new ClassDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, openBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override TypeDeclarationSyntax WithMembersCore(SyntaxList<MemberDeclarationSyntax> members)
	{
		return WithMembers(members);
	}

	public new ClassDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithCloseBraceTokenCore(SyntaxToken closeBraceToken)
	{
		return WithCloseBraceToken(closeBraceToken);
	}

	public new ClassDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, closeBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new ClassDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Keyword, Identifier, TypeParameterList, ParameterList, BaseList, ConstraintClauses, OpenBraceToken, Members, CloseBraceToken, semicolonToken);
	}

	internal override MemberDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new ClassDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override MemberDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new ClassDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override TypeDeclarationSyntax AddTypeParameterListParametersCore(params TypeParameterSyntax[] items)
	{
		return AddTypeParameterListParameters(items);
	}

	public new ClassDeclarationSyntax AddTypeParameterListParameters(params TypeParameterSyntax[] items)
	{
		TypeParameterListSyntax typeParameterListSyntax = TypeParameterList ?? SyntaxFactory.TypeParameterList();
		return WithTypeParameterList(typeParameterListSyntax.WithParameters(typeParameterListSyntax.Parameters.AddRange(items)));
	}

	internal override TypeDeclarationSyntax AddParameterListParametersCore(params ParameterSyntax[] items)
	{
		return AddParameterListParameters(items);
	}

	public new ClassDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		ParameterListSyntax parameterListSyntax = ParameterList ?? SyntaxFactory.ParameterList();
		return WithParameterList(parameterListSyntax.WithParameters(parameterListSyntax.Parameters.AddRange(items)));
	}

	internal override BaseTypeDeclarationSyntax AddBaseListTypesCore(params BaseTypeSyntax[] items)
	{
		return AddBaseListTypes(items);
	}

	public new ClassDeclarationSyntax AddBaseListTypes(params BaseTypeSyntax[] items)
	{
		BaseListSyntax baseListSyntax = BaseList ?? SyntaxFactory.BaseList();
		return WithBaseList(baseListSyntax.WithTypes(baseListSyntax.Types.AddRange(items)));
	}

	internal override TypeDeclarationSyntax AddConstraintClausesCore(params TypeParameterConstraintClauseSyntax[] items)
	{
		return AddConstraintClauses(items);
	}

	public new ClassDeclarationSyntax AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
	{
		return WithConstraintClauses(ConstraintClauses.AddRange(items));
	}

	internal override TypeDeclarationSyntax AddMembersCore(params MemberDeclarationSyntax[] items)
	{
		return AddMembers(items);
	}

	public new ClassDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
