using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Footers;

public class FooterBlock : ContainerBlock
{
	public char OpeningCharacter { get; set; }

	public FooterBlock(BlockParser parser)
		: base(parser)
	{
	}
}
