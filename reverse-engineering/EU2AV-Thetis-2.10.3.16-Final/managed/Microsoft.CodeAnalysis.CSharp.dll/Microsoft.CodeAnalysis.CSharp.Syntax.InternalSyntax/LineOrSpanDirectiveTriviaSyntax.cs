namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class LineOrSpanDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public abstract SyntaxToken LineKeyword { get; }

	public abstract SyntaxToken? File { get; }

	internal LineOrSpanDirectiveTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal LineOrSpanDirectiveTriviaSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
