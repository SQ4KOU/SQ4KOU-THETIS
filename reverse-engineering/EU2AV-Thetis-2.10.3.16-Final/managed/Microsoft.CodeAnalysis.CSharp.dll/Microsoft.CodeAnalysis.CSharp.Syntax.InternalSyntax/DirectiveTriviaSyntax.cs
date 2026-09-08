namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class DirectiveTriviaSyntax : StructuredTriviaSyntax
{
	public sealed override bool IsDirective => true;

	public abstract SyntaxToken HashToken { get; }

	public abstract SyntaxToken EndOfDirectiveToken { get; }

	public abstract bool IsActive { get; }

	internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
	{
		return stack.Add(new Directive(this));
	}

	internal DirectiveTriviaSyntax(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
		: base(kind, diagnostics, annotations)
	{
		SetFlags(NodeFlags.ContainsDirectives);
	}

	internal DirectiveTriviaSyntax(SyntaxKind kind)
		: base(kind)
	{
		SetFlags(NodeFlags.ContainsDirectives);
	}
}
