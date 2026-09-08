namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class ExpressionOrPatternSyntax : CSharpSyntaxNode
{
	internal ExpressionOrPatternSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal ExpressionOrPatternSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
