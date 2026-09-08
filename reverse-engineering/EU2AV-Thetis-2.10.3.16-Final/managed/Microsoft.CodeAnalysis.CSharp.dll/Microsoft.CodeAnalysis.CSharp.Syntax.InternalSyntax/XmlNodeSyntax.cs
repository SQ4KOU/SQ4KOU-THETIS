namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class XmlNodeSyntax : CSharpSyntaxNode
{
	internal XmlNodeSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal XmlNodeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
