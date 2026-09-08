using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseNamespaceDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxToken NamespaceKeyword { get; }

	public abstract NameSyntax Name { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<ExternAliasDirectiveSyntax> Externs { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<UsingDirectiveSyntax> Usings { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal BaseNamespaceDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseNamespaceDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
