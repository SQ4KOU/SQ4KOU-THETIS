using Markdig.Parsers;

namespace Markdig.Syntax;

public sealed class EmptyBlock : LeafBlock
{
	public EmptyBlock(BlockParser? parser)
		: base(parser)
	{
	}
}
