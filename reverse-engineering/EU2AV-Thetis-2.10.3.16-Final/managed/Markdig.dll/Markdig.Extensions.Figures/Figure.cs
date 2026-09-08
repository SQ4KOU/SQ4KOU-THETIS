using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Figures;

public class Figure : ContainerBlock
{
	public int OpeningCharacterCount { get; set; }

	public char OpeningCharacter { get; set; }

	public Figure(BlockParser parser)
		: base(parser)
	{
	}
}
