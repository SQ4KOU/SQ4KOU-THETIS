using System.Collections.Generic;

namespace Markdig.Parsers;

public class InlineParserList : ParserList<InlineParser, InlineProcessor>
{
	public IPostInlineProcessor[] PostInlineProcessors { get; private set; }

	public InlineParserList(IEnumerable<InlineParser> parsers)
		: base(parsers)
	{
		List<IPostInlineProcessor> list = new List<IPostInlineProcessor>();
		using (List<InlineParser>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is IPostInlineProcessor item)
				{
					list.Add(item);
				}
			}
		}
		PostInlineProcessors = list.ToArray();
	}
}
