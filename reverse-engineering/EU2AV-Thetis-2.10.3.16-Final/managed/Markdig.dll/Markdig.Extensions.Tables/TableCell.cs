using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Tables;

public class TableCell : ContainerBlock
{
	public int ColumnIndex { get; set; }

	public int ColumnSpan { get; set; }

	public int RowSpan { get; set; }

	public bool AllowClose { get; set; }

	public TableCell()
		: this(null)
	{
	}

	public TableCell(BlockParser? parser)
		: base(parser)
	{
		AllowClose = true;
		ColumnSpan = 1;
		ColumnIndex = -1;
		RowSpan = 1;
	}
}
