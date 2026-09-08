using System.Diagnostics;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("{Original} -> {Transcoded}")]
public class HtmlEntityInline : LeafInline
{
	public StringSlice Original { get; set; }

	public StringSlice Transcoded { get; set; }
}
