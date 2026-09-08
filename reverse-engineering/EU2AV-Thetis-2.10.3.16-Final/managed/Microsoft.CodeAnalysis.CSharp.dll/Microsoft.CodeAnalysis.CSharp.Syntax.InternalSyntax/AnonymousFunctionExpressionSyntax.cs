using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class AnonymousFunctionExpressionSyntax : ExpressionSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract BlockSyntax? Block { get; }

	public abstract ExpressionSyntax? ExpressionBody { get; }

	internal AnonymousFunctionExpressionSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal AnonymousFunctionExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
