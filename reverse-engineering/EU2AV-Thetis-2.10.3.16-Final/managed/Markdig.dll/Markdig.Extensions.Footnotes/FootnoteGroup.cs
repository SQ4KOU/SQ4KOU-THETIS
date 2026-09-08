using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Footnotes;

public class FootnoteGroup : ContainerBlock
{
	internal int CurrentOrder { get; set; }

	public FootnoteGroup(BlockParser parser)
		: base(parser)
	{
	}
}
