using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.DefinitionLists;

public class DefinitionTerm : LeafBlock
{
	public DefinitionTerm(BlockParser parser)
		: base(parser)
	{
		base.ProcessInlines = true;
	}
}
