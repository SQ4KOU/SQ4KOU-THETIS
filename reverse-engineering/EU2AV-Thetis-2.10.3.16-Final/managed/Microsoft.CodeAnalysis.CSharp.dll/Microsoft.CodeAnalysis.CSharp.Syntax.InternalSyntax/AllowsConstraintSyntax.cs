namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class AllowsConstraintSyntax : CSharpSyntaxNode
{
	internal AllowsConstraintSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal AllowsConstraintSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
