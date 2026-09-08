namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class SelectOrGroupClauseSyntax : CSharpSyntaxNode
{
	internal SelectOrGroupClauseSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal SelectOrGroupClauseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
