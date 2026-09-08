using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Mathematics;

public class MathBlock : FencedCodeBlock
{
	public MathBlock(BlockParser parser)
		: base(parser)
	{
	}
}
