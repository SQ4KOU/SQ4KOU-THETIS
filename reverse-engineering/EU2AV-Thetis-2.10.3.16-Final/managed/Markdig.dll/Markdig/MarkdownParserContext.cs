using System.Collections.Generic;

namespace Markdig;

public class MarkdownParserContext
{
	public Dictionary<object, object> Properties { get; }

	public MarkdownParserContext()
	{
		Properties = new Dictionary<object, object>();
	}
}
