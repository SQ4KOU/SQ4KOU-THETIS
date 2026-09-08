namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class CollectionElementSyntax : CSharpSyntaxNode
{
	internal CollectionElementSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal CollectionElementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
