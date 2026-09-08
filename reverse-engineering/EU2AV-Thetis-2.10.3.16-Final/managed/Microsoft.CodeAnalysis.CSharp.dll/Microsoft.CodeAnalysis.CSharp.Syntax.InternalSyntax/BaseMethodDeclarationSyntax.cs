namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseMethodDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract ParameterListSyntax ParameterList { get; }

	public abstract BlockSyntax? Body { get; }

	public abstract ArrowExpressionClauseSyntax? ExpressionBody { get; }

	public abstract SyntaxToken? SemicolonToken { get; }

	internal BaseMethodDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseMethodDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
