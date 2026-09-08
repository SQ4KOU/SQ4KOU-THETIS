namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseExpressionColonSyntax : CSharpSyntaxNode
{
	public abstract ExpressionSyntax Expression { get; }

	public abstract SyntaxToken ColonToken { get; }

	internal BaseExpressionColonSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseExpressionColonSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
