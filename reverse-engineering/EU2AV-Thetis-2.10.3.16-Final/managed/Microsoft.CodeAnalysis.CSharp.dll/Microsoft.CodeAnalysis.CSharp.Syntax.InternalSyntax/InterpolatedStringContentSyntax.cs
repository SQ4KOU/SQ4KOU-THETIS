namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class InterpolatedStringContentSyntax : CSharpSyntaxNode
{
	internal InterpolatedStringContentSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InterpolatedStringContentSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
