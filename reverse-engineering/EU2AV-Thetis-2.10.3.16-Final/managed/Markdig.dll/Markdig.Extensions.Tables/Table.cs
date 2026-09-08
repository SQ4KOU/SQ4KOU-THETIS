using System.Collections.Generic;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Tables;

public class Table : ContainerBlock
{
	public List<TableColumnDefinition> ColumnDefinitions { get; } = new List<TableColumnDefinition>();

	public Table()
		: base(null)
	{
	}

	public Table(BlockParser? parser)
		: base(parser)
	{
	}

	public bool IsValid()
	{
		if (base.Count == 0)
		{
			return false;
		}
		int count = ColumnDefinitions.Count;
		int[] array = new int[base.Count];
		for (int i = 0; i < base.Count; i++)
		{
			TableRow tableRow = (TableRow)base[i];
			for (int j = 0; j < tableRow.Count; j++)
			{
				TableCell tableCell = (TableCell)tableRow[j];
				array[i] += tableCell.ColumnSpan;
				for (int num = tableCell.RowSpan - 1; num > 0; num--)
				{
					if (i + num > array.Length - 1)
					{
						return false;
					}
					array[i + num] += tableCell.ColumnSpan;
				}
			}
			if (array[i] > count)
			{
				return false;
			}
		}
		return true;
	}

	public void NormalizeUsingMaxWidth()
	{
		int num = 0;
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TableRow tableRow && tableRow.Count > num)
			{
				num = tableRow.Count;
			}
		}
		for (int j = 0; j < base.Count; j++)
		{
			if (base[j] is TableRow { Count: var k } tableRow2)
			{
				for (; k < num; k++)
				{
					tableRow2.Add(new TableCell());
				}
			}
		}
	}

	public void NormalizeUsingHeaderRow()
	{
		if (base.Count == 0)
		{
			return;
		}
		int num = 0;
		if (base[0] is TableRow tableRow)
		{
			num = tableRow.Count;
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TableRow { Count: var j } tableRow2)
			{
				for (; j < num; j++)
				{
					tableRow2.Add(new TableCell());
				}
				for (int k = num; k < tableRow2.Count; k++)
				{
					tableRow2.RemoveAt(k);
				}
			}
		}
	}
}
