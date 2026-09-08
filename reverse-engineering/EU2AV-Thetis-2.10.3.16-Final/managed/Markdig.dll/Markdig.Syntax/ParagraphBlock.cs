using Markdig.Parsers;

namespace Markdig.Syntax;

public class ParagraphBlock : LeafBlock
{
	public int LastLine => base.Line + Lines.Count - 1;

	public ParagraphBlock()
		: this(null)
	{
	}

	public ParagraphBlock(BlockParser? parser)
		: base(parser)
	{
		base.ProcessInlines = true;
		base.IsParagraphBlock = true;
	}
}
