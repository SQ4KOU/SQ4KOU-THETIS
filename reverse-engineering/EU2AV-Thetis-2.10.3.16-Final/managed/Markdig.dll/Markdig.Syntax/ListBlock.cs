using Markdig.Parsers;

namespace Markdig.Syntax;

public class ListBlock : ContainerBlock
{
	public bool IsOrdered { get; set; }

	public char BulletType { get; set; }

	public string? OrderedStart { get; set; }

	public string? DefaultOrderedStart { get; set; }

	public char OrderedDelimiter { get; set; }

	public bool IsLoose { get; set; }

	internal int CountAllBlankLines { get; set; }

	internal int CountBlankLinesReset { get; set; }

	public ListBlock(BlockParser parser)
		: base(parser)
	{
	}
}
