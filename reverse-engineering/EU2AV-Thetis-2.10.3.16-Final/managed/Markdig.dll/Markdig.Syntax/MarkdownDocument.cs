using System.Collections.Generic;

namespace Markdig.Syntax;

public class MarkdownDocument : ContainerBlock
{
	public int LineCount;

	public List<int>? LineStartIndexes { get; set; }

	public MarkdownDocument()
		: base(null)
	{
	}
}
