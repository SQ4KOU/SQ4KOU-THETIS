using System;
using System.Globalization;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Tables;

public class HtmlTableRenderer : HtmlObjectRenderer<Table>
{
	protected override void Write(HtmlRenderer renderer, Table table)
	{
		if (renderer.EnableHtmlForBlock)
		{
			renderer.EnsureLine();
			renderer.Write("<table").WriteAttributes(table).WriteLine('>');
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			foreach (TableColumnDefinition columnDefinition in table.ColumnDefinitions)
			{
				if (columnDefinition.Width != 0f && columnDefinition.Width != 1f)
				{
					flag4 = true;
					break;
				}
			}
			if (flag4)
			{
				foreach (TableColumnDefinition columnDefinition2 in table.ColumnDefinitions)
				{
					double num = Math.Round(columnDefinition2.Width * 100f) / 100.0;
					string text = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", num);
					renderer.WriteLine("<col style=\"width:" + text + "%\" />");
				}
			}
			foreach (TableRow item in table)
			{
				if (item.IsHeader)
				{
					if (!flag2)
					{
						renderer.WriteLine("<thead>");
						flag3 = true;
					}
					flag2 = true;
				}
				else if (!flag)
				{
					if (flag3)
					{
						renderer.WriteLine("</thead>");
						flag3 = false;
					}
					renderer.WriteLine("<tbody>");
					flag = true;
				}
				renderer.Write("<tr").WriteAttributes(item).WriteLine('>');
				for (int i = 0; i < item.Count; i++)
				{
					TableCell tableCell = (TableCell)item[i];
					renderer.EnsureLine();
					renderer.Write(item.IsHeader ? "<th" : "<td");
					if (tableCell.ColumnSpan != 1)
					{
						renderer.Write($" colspan=\"{tableCell.ColumnSpan}\"");
					}
					if (tableCell.RowSpan != 1)
					{
						renderer.Write($" rowspan=\"{tableCell.RowSpan}\"");
					}
					if (table.ColumnDefinitions.Count > 0)
					{
						int num2 = ((tableCell.ColumnIndex < 0 || tableCell.ColumnIndex >= table.ColumnDefinitions.Count) ? i : tableCell.ColumnIndex);
						num2 = ((num2 >= table.ColumnDefinitions.Count) ? (table.ColumnDefinitions.Count - 1) : num2);
						TableColumnAlign? alignment = table.ColumnDefinitions[num2].Alignment;
						if (alignment.HasValue)
						{
							switch (alignment)
							{
							case TableColumnAlign.Center:
								renderer.Write(" style=\"text-align: center;\"");
								break;
							case TableColumnAlign.Right:
								renderer.Write(" style=\"text-align: right;\"");
								break;
							case TableColumnAlign.Left:
								renderer.Write(" style=\"text-align: left;\"");
								break;
							}
						}
					}
					renderer.WriteAttributes(tableCell);
					renderer.Write('>');
					bool implicitParagraph = renderer.ImplicitParagraph;
					if (tableCell.Count == 1)
					{
						renderer.ImplicitParagraph = true;
					}
					renderer.Write(tableCell);
					renderer.ImplicitParagraph = implicitParagraph;
					renderer.WriteLine(item.IsHeader ? "</th>" : "</td>");
				}
				renderer.WriteLine("</tr>");
			}
			if (flag)
			{
				renderer.WriteLine("</tbody>");
			}
			else if (flag3)
			{
				renderer.WriteLine("</thead>");
			}
			renderer.WriteLine("</table>");
			return;
		}
		bool implicitParagraph2 = renderer.ImplicitParagraph;
		renderer.ImplicitParagraph = true;
		foreach (TableRow item2 in table)
		{
			for (int j = 0; j < item2.Count; j++)
			{
				TableCell obj = (TableCell)item2[j];
				renderer.Write(obj);
				renderer.Write(' ');
			}
		}
		renderer.ImplicitParagraph = implicitParagraph2;
	}
}
