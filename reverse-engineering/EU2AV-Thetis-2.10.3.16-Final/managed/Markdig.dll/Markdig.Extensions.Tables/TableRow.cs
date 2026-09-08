using Markdig.Syntax;

namespace Markdig.Extensions.Tables;

public class TableRow : ContainerBlock
{
	public bool IsHeader { get; set; }

	public TableRow()
		: base(null)
	{
	}
}
