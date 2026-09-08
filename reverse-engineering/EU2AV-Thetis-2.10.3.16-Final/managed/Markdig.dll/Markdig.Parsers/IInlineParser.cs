using Markdig.Helpers;

namespace Markdig.Parsers;

public interface IInlineParser<in TProcessor> : IMarkdownParser<TProcessor>
{
	bool Match(InlineProcessor processor, ref StringSlice slice);
}
