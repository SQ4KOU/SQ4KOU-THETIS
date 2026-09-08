using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BaseNamespaceDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxToken NamespaceKeyword { get; }

	public abstract NameSyntax Name { get; }

	public abstract SyntaxList<ExternAliasDirectiveSyntax> Externs { get; }

	public abstract SyntaxList<UsingDirectiveSyntax> Usings { get; }

	public abstract SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal BaseNamespaceDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	public BaseNamespaceDeclarationSyntax WithNamespaceKeyword(SyntaxToken namespaceKeyword)
	{
		return WithNamespaceKeywordCore(namespaceKeyword);
	}

	internal abstract BaseNamespaceDeclarationSyntax WithNamespaceKeywordCore(SyntaxToken namespaceKeyword);

	public BaseNamespaceDeclarationSyntax WithName(NameSyntax name)
	{
		return WithNameCore(name);
	}

	internal abstract BaseNamespaceDeclarationSyntax WithNameCore(NameSyntax name);

	public BaseNamespaceDeclarationSyntax WithExterns(SyntaxList<ExternAliasDirectiveSyntax> externs)
	{
		return WithExternsCore(externs);
	}

	internal abstract BaseNamespaceDeclarationSyntax WithExternsCore(SyntaxList<ExternAliasDirectiveSyntax> externs);

	public BaseNamespaceDeclarationSyntax AddExterns(params ExternAliasDirectiveSyntax[] items)
	{
		return AddExternsCore(items);
	}

	internal abstract BaseNamespaceDeclarationSyntax AddExternsCore(params ExternAliasDirectiveSyntax[] items);

	public BaseNamespaceDeclarationSyntax WithUsings(SyntaxList<UsingDirectiveSyntax> usings)
	{
		return WithUsingsCore(usings);
	}

	internal abstract BaseNamespaceDeclarationSyntax WithUsingsCore(SyntaxList<UsingDirectiveSyntax> usings);

	public BaseNamespaceDeclarationSyntax AddUsings(params UsingDirectiveSyntax[] items)
	{
		return AddUsingsCore(items);
	}

	internal abstract BaseNamespaceDeclarationSyntax AddUsingsCore(params UsingDirectiveSyntax[] items);

	public BaseNamespaceDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return WithMembersCore(members);
	}

	internal abstract BaseNamespaceDeclarationSyntax WithMembersCore(SyntaxList<MemberDeclarationSyntax> members);

	public BaseNamespaceDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return AddMembersCore(items);
	}

	internal abstract BaseNamespaceDeclarationSyntax AddMembersCore(params MemberDeclarationSyntax[] items);

	public new BaseNamespaceDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return (BaseNamespaceDeclarationSyntax)WithAttributeListsCore(attributeLists);
	}

	public new BaseNamespaceDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return (BaseNamespaceDeclarationSyntax)WithModifiersCore(modifiers);
	}

	public new BaseNamespaceDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return (BaseNamespaceDeclarationSyntax)AddAttributeListsCore(items);
	}

	public new BaseNamespaceDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return (BaseNamespaceDeclarationSyntax)AddModifiersCore(items);
	}
}
