namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class ExpressionSyntax : ExpressionOrPatternSyntax
{
	internal ExpressionSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal ExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
