using System.Collections.Generic;
using Markdig.Parsers;

namespace Markdig.Syntax;

public class QuoteBlock : ContainerBlock
{
	private List<QuoteBlockLine> Trivia => GetOrSetDerivedTrivia<List<QuoteBlockLine>>();

	public List<QuoteBlockLine> QuoteLines => Trivia;

	public char QuoteChar { get; set; }

	public QuoteBlock(BlockParser? parser)
		: base(parser)
	{
	}
}
