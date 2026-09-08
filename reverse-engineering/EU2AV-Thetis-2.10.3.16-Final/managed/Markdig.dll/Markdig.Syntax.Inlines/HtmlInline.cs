using System.Diagnostics;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("{Tag}")]
public class HtmlInline : LeafInline
{
	public string Tag { get; set; }

	public HtmlInline(string tag)
	{
		Tag = tag;
	}
}
