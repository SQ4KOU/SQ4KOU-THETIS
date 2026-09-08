namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BasePropertyDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract TypeSyntax Type { get; }

	public abstract ExplicitInterfaceSpecifierSyntax? ExplicitInterfaceSpecifier { get; }

	public abstract AccessorListSyntax? AccessorList { get; }

	internal BasePropertyDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BasePropertyDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
