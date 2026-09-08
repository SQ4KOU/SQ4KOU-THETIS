using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FileScopedNamespaceDeclarationSyntax : BaseNamespaceDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken namespaceKeyword;

	internal readonly NameSyntax name;

	internal readonly SyntaxToken semicolonToken;

	internal readonly GreenNode? externs;

	internal readonly GreenNode? usings;

	internal readonly GreenNode? members;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override SyntaxToken NamespaceKeyword => namespaceKeyword;

	public override NameSyntax Name => name;

	public SyntaxToken SemicolonToken => semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> Externs => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax>(externs);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> Usings => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax>(usings);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>(members);

	internal FileScopedNamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken semicolonToken, GreenNode? externs, GreenNode? usings, GreenNode? members, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
	}

	internal FileScopedNamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken semicolonToken, GreenNode? externs, GreenNode? usings, GreenNode? members, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
	}

	internal FileScopedNamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken semicolonToken, GreenNode? externs, GreenNode? usings, GreenNode? members)
		: base(kind)
	{
		base.SlotCount = 8;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(namespaceKeyword);
		this.namespaceKeyword = namespaceKeyword;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
		if (externs != null)
		{
			AdjustFlagsAndWidth(externs);
			this.externs = externs;
		}
		if (usings != null)
		{
			AdjustFlagsAndWidth(usings);
			this.usings = usings;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			this.members = members;
		}
	}

	internal override GreenNode? GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => namespaceKeyword, 
			3 => name, 
			4 => semicolonToken, 
			5 => externs, 
			6 => usings, 
			7 => members, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFileScopedNamespaceDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFileScopedNamespaceDeclaration(this);
	}

	public FileScopedNamespaceDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken semicolonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || namespaceKeyword != NamespaceKeyword || name != Name || semicolonToken != SemicolonToken || externs != Externs || usings != Usings || members != Members)
		{
			FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax = SyntaxFactory.FileScopedNamespaceDeclaration(attributeLists, modifiers, namespaceKeyword, name, semicolonToken, externs, usings, members);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				fileScopedNamespaceDeclarationSyntax = fileScopedNamespaceDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				fileScopedNamespaceDeclarationSyntax = fileScopedNamespaceDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return fileScopedNamespaceDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new FileScopedNamespaceDeclarationSyntax(base.Kind, attributeLists, modifiers, namespaceKeyword, name, semicolonToken, externs, usings, members, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new FileScopedNamespaceDeclarationSyntax(base.Kind, attributeLists, modifiers, namespaceKeyword, name, semicolonToken, externs, usings, members, GetDiagnostics(), annotations);
	}
}
