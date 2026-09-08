using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

public class LineBreakInline : LeafInline
{
	public bool IsHard { get; set; }

	public bool IsBackslash { get; set; }

	public NewLine NewLine { get; set; }
}
