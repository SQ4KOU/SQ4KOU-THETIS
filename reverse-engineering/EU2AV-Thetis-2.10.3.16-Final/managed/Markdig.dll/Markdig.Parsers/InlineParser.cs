using Markdig.Helpers;

namespace Markdig.Parsers;

public abstract class InlineParser : ParserBase<InlineProcessor>, IInlineParser<InlineProcessor>, IMarkdownParser<InlineProcessor>
{
	public abstract bool Match(InlineProcessor processor, ref StringSlice slice);
}
