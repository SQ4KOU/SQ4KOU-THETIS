namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseObjectCreationExpressionSyntax : ExpressionSyntax
{
	public abstract SyntaxToken NewKeyword { get; }

	public abstract ArgumentListSyntax? ArgumentList { get; }

	public abstract InitializerExpressionSyntax? Initializer { get; }

	internal BaseObjectCreationExpressionSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseObjectCreationExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}
}
