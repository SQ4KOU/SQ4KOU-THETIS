using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.DefinitionLists;

public class DefinitionList : ContainerBlock
{
	public DefinitionList(BlockParser parser)
		: base(parser)
	{
	}
}
