namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseFieldDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract VariableDeclarationSyntax Declaration { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseFieldDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseFieldDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
