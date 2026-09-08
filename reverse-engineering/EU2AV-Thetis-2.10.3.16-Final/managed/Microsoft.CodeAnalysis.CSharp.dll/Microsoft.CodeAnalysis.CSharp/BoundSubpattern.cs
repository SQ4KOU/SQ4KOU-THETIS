namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundSubpattern : BoundNode
{
	public BoundPattern Pattern { get; }

	protected BoundSubpattern(BoundKind kind, SyntaxNode syntax, BoundPattern pattern, bool hasErrors = false)
		: base(kind, syntax, hasErrors)
	{
		Pattern = pattern;
	}
}
