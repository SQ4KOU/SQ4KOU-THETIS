namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class StructuredTriviaSyntax : CSharpSyntaxNode
{
	public sealed override bool IsStructuredTrivia => true;

	internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
		: base(kind, diagnostics, annotations)
	{
		SetFlags(NodeFlags.ContainsStructuredTrivia);
		if (base.Kind == SyntaxKind.SkippedTokensTrivia)
		{
			SetFlags(NodeFlags.ContainsSkippedText);
		}
	}
}
