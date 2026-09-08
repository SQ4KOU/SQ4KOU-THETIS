namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class PatternSyntax : ExpressionOrPatternSyntax
{
	internal PatternSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal PatternSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
