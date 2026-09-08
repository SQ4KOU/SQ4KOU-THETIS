using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NamespaceDeclarationSyntax : BaseNamespaceDeclarationSyntax
{
	internal readonly GreenNode? attributeLists;

	internal readonly GreenNode? modifiers;

	internal readonly SyntaxToken namespaceKeyword;

	internal readonly NameSyntax name;

	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode? externs;

	internal readonly GreenNode? usings;

	internal readonly GreenNode? members;

	internal readonly SyntaxToken closeBraceToken;

	internal readonly SyntaxToken? semicolonToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public override SyntaxToken NamespaceKeyword => namespaceKeyword;

	public override NameSyntax Name => name;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> Externs => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax>(externs);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> Usings => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax>(usings);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax>(members);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	public SyntaxToken? SemicolonToken => semicolonToken;

	internal NamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode? externs, GreenNode? usings, GreenNode? members, SyntaxToken closeBraceToken, SyntaxToken? semicolonToken, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
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
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal NamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode? externs, GreenNode? usings, GreenNode? members, SyntaxToken closeBraceToken, SyntaxToken? semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
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
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
		}
	}

	internal NamespaceDeclarationSyntax(SyntaxKind kind, GreenNode? attributeLists, GreenNode? modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, GreenNode? externs, GreenNode? usings, GreenNode? members, SyntaxToken closeBraceToken, SyntaxToken? semicolonToken)
		: base(kind)
	{
		base.SlotCount = 10;
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
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
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
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			this.semicolonToken = semicolonToken;
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
			4 => openBraceToken, 
			5 => externs, 
			6 => usings, 
			7 => members, 
			8 => closeBraceToken, 
			9 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax(this, parent, position);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNamespaceDeclaration(this);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNamespaceDeclaration(this);
	}

	public NamespaceDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> externs, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> usings, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || namespaceKeyword != NamespaceKeyword || name != Name || openBraceToken != OpenBraceToken || externs != Externs || usings != Usings || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = SyntaxFactory.NamespaceDeclaration(attributeLists, modifiers, namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				namespaceDeclarationSyntax = namespaceDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				namespaceDeclarationSyntax = namespaceDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return namespaceDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new NamespaceDeclarationSyntax(base.Kind, attributeLists, modifiers, namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new NamespaceDeclarationSyntax(base.Kind, attributeLists, modifiers, namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken, GetDiagnostics(), annotations);
	}
}
