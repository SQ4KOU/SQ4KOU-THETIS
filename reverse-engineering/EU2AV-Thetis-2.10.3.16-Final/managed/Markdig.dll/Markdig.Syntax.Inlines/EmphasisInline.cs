using System.Diagnostics;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("{DelimiterChar} Count: {DelimiterCount}")]
public class EmphasisInline : ContainerInline
{
	public char DelimiterChar { get; set; }

	public int DelimiterCount { get; set; }
}
