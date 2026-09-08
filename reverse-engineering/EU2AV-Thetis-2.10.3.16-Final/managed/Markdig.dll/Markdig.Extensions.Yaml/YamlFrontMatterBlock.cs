using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Yaml;

public class YamlFrontMatterBlock : CodeBlock
{
	public YamlFrontMatterBlock(BlockParser parser)
		: base(parser)
	{
	}
}
