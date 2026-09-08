using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Figures;

public class FigureCaption : LeafBlock
{
	public FigureCaption(BlockParser parser)
		: base(parser)
	{
		base.ProcessInlines = true;
	}
}
