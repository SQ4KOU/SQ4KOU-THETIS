using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Tables;

public class PipeTableDelimiterInline : DelimiterInline
{
	public int LocalLineIndex { get; set; }

	public PipeTableDelimiterInline(InlineParser parser)
		: base(parser)
	{
	}

	public override string ToLiteral()
	{
		return "|";
	}
}
