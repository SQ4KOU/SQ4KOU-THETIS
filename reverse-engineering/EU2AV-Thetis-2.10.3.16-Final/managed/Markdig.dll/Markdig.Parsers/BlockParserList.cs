using System.Collections.Generic;

namespace Markdig.Parsers;

public class BlockParserList : ParserList<BlockParser, BlockProcessor>
{
	public BlockParserList(IEnumerable<BlockParser> parsers)
		: base(parsers)
	{
	}
}
