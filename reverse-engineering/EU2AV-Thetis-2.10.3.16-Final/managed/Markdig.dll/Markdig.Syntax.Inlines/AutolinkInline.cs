using System.Diagnostics;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("<{Url}>")]
public sealed class AutolinkInline : LeafInline
{
	public bool IsEmail { get; set; }

	public string Url { get; set; }

	public AutolinkInline(string url)
	{
		Url = url;
	}

	public override string ToString()
	{
		return Url;
	}
}
