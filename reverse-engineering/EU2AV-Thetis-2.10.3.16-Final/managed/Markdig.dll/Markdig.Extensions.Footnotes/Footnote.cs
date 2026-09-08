using System.Collections.Generic;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Footnotes;

public class Footnote : ContainerBlock
{
	public SourceSpan LabelSpan;

	public string? Label { get; set; }

	public int Order { get; set; }

	public List<FootnoteLink> Links { get; } = new List<FootnoteLink>();

	internal bool IsLastLineEmpty { get; set; }

	public Footnote(BlockParser parser)
		: base(parser)
	{
		Order = -1;
	}
}
