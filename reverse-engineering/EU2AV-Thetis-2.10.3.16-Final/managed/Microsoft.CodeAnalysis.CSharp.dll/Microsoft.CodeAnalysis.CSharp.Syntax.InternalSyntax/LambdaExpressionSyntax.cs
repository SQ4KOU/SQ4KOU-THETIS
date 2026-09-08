using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class LambdaExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract SyntaxToken ArrowToken { get; }

	internal LambdaExpressionSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal LambdaExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
