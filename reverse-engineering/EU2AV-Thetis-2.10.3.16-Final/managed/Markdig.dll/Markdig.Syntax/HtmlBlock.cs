using Markdig.Parsers;

namespace Markdig.Syntax;

public class HtmlBlock : LeafBlock
{
	public HtmlBlockType Type { get; set; }

	public HtmlBlock(BlockParser? parser)
		: base(parser)
	{
	}
}
