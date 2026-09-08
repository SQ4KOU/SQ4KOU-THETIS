using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Mathematics;

public class MathInline : LeafInline
{
	public StringSlice Content;

	public char Delimiter { get; set; }

	public int DelimiterCount { get; set; }
}
