using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.DefinitionLists;

public class DefinitionItem : ContainerBlock
{
	public char OpeningCharacter { get; set; }

	public DefinitionItem(BlockParser parser)
		: base(parser)
	{
	}
}
