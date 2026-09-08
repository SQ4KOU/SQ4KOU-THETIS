using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public class ThematicBreakBlock : LeafBlock
{
	public StringSlice Content;

	public char ThematicChar { get; set; }

	public int ThematicCharCount { get; set; }

	public ThematicBreakBlock(BlockParser parser)
		: base(parser)
	{
	}
}
