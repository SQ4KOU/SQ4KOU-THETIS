using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Extensions.Tables;

internal sealed class GridTableState(int start, bool expectRow)
{
	public sealed class ColumnSlice(int start, int end, TableColumnAlign? align)
	{
		public int Start { get; } = start;

		public int End { get; } = end;

		public TableColumnAlign? Align { get; } = align;

		public int CurrentColumnSpan { get; set; } = -1;

		public int PreviousColumnSpan { get; set; }

		public BlockProcessor? BlockProcessor { get; set; }

		public TableCell? CurrentCell { get; set; }
	}

	public StringLineGroup Lines;

	public int Start { get; } = start;

	public List<ColumnSlice>? ColumnSlices { get; private set; }

	public bool ExpectRow { get; } = expectRow;

	public int StartRowGroup { get; set; }

	public void AddLine(ref StringSlice line)
	{
		if (Lines.Lines == null)
		{
			Lines = new StringLineGroup(4);
		}
		Lines.Add(line);
	}

	public void AddColumn(int start, int end, TableColumnAlign? align)
	{
		if (ColumnSlices == null)
		{
			List<ColumnSlice> list = (ColumnSlices = new List<ColumnSlice>());
		}
		ColumnSlices.Add(new ColumnSlice(start, end, align));
	}
}
