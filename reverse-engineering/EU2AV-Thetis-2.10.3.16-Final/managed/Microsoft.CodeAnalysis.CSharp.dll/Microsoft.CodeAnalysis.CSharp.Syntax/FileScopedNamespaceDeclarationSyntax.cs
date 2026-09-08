using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FileScopedNamespaceDeclarationSyntax : BaseNamespaceDeclarationSyntax
{
	private SyntaxNode? attributeLists;

	private NameSyntax? name;

	private SyntaxNode? externs;

	private SyntaxNode? usings;

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

	public override SyntaxToken NamespaceKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FileScopedNamespaceDeclarationSyntax)base.Green).namespaceKeyword, GetChildPosition(2), GetChildIndex(2));

	public override NameSyntax Name => GetRed(ref name, 3);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FileScopedNamespaceDeclarationSyntax)base.Green).semicolonToken, GetChildPosition(4), GetChildIndex(4));

	public override SyntaxList<ExternAliasDirectiveSyntax> Externs => new SyntaxList<ExternAliasDirectiveSyntax>(GetRed(ref externs, 5));

	public override SyntaxList<UsingDirectiveSyntax> Usings => new SyntaxList<UsingDirectiveSyntax>(GetRed(ref usings, 6));

	public override SyntaxList<MemberDeclarationSyntax> Members => new SyntaxList<MemberDeclarationSyntax>(GetRed(ref members, 7));

	internal FileScopedNamespaceDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode? parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode? GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			3 => GetRed(ref name, 3), 
			5 => GetRed(ref externs, 5), 
			6 => GetRed(ref usings, 6), 
			7 => GetRed(ref members, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode? GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			3 => name, 
			5 => externs, 
			6 => usings, 
			7 => members, 
			_ => null, 
		};
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFileScopedNamespaceDeclaration(this);
	}

	public override TResult? Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor) where TResult : default
	{
		return visitor.VisitFileScopedNamespaceDeclaration(this);
	}

	public FileScopedNamespaceDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken semicolonToken, SyntaxList<ExternAliasDirectiveSyntax> externs, SyntaxList<UsingDirectiveSyntax> usings, SyntaxList<MemberDeclarationSyntax> members)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || namespaceKeyword != NamespaceKeyword || name != Name || semicolonToken != SemicolonToken || externs != Externs || usings != Usings || members != Members)
		{
			FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax = SyntaxFactory.FileScopedNamespaceDeclaration(attributeLists, modifiers, namespaceKeyword, name, semicolonToken, externs, usings, members);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations == null || annotations.Length == 0)
			{
				return fileScopedNamespaceDeclarationSyntax;
			}
			return fileScopedNamespaceDeclarationSyntax.WithAnnotations(annotations);
		}
		return this;
	}

	internal override MemberDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new FileScopedNamespaceDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, NamespaceKeyword, Name, SemicolonToken, Externs, Usings, Members);
	}

	internal override MemberDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new FileScopedNamespaceDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, NamespaceKeyword, Name, SemicolonToken, Externs, Usings, Members);
	}

	internal override BaseNamespaceDeclarationSyntax WithNamespaceKeywordCore(SyntaxToken namespaceKeyword)
	{
		return WithNamespaceKeyword(namespaceKeyword);
	}

	public new FileScopedNamespaceDeclarationSyntax WithNamespaceKeyword(SyntaxToken namespaceKeyword)
	{
		return Update(AttributeLists, Modifiers, namespaceKeyword, Name, SemicolonToken, Externs, Usings, Members);
	}

	internal override BaseNamespaceDeclarationSyntax WithNameCore(NameSyntax name)
	{
		return WithName(name);
	}

	public new FileScopedNamespaceDeclarationSyntax WithName(NameSyntax name)
	{
		return Update(AttributeLists, Modifiers, NamespaceKeyword, name, SemicolonToken, Externs, Usings, Members);
	}

	public FileScopedNamespaceDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, NamespaceKeyword, Name, semicolonToken, Externs, Usings, Members);
	}

	internal override BaseNamespaceDeclarationSyntax WithExternsCore(SyntaxList<ExternAliasDirectiveSyntax> externs)
	{
		return WithExterns(externs);
	}

	public new FileScopedNamespaceDeclarationSyntax WithExterns(SyntaxList<ExternAliasDirectiveSyntax> externs)
	{
		return Update(AttributeLists, Modifiers, NamespaceKeyword, Name, SemicolonToken, externs, Usings, Members);
	}

	internal override BaseNamespaceDeclarationSyntax WithUsingsCore(SyntaxList<UsingDirectiveSyntax> usings)
	{
		return WithUsings(usings);
	}

	public new FileScopedNamespaceDeclarationSyntax WithUsings(SyntaxList<UsingDirectiveSyntax> usings)
	{
		return Update(AttributeLists, Modifiers, NamespaceKeyword, Name, SemicolonToken, Externs, usings, Members);
	}

	internal override BaseNamespaceDeclarationSyntax WithMembersCore(SyntaxList<MemberDeclarationSyntax> members)
	{
		return WithMembers(members);
	}

	public new FileScopedNamespaceDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return Update(AttributeLists, Modifiers, NamespaceKeyword, Name, SemicolonToken, Externs, Usings, members);
	}

	internal override MemberDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new FileScopedNamespaceDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override MemberDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new FileScopedNamespaceDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseNamespaceDeclarationSyntax AddExternsCore(params ExternAliasDirectiveSyntax[] items)
	{
		return AddExterns(items);
	}

	public new FileScopedNamespaceDeclarationSyntax AddExterns(params ExternAliasDirectiveSyntax[] items)
	{
		return WithExterns(Externs.AddRange(items));
	}

	internal override BaseNamespaceDeclarationSyntax AddUsingsCore(params UsingDirectiveSyntax[] items)
	{
		return AddUsings(items);
	}

	public new FileScopedNamespaceDeclarationSyntax AddUsings(params UsingDirectiveSyntax[] items)
	{
		return WithUsings(Usings.AddRange(items));
	}

	internal override BaseNamespaceDeclarationSyntax AddMembersCore(params MemberDeclarationSyntax[] items)
	{
		return AddMembers(items);
	}

	public new FileScopedNamespaceDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
