using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class ListRenderer : HtmlObjectRenderer<ListBlock>
{
	protected override void Write(HtmlRenderer renderer, ListBlock listBlock)
	{
		renderer.EnsureLine();
		if (renderer.EnableHtmlForBlock)
		{
			if (listBlock.IsOrdered)
			{
				renderer.Write("<ol");
				if (listBlock.BulletType != '1')
				{
					renderer.WriteRaw(" type=\"");
					renderer.WriteRaw(listBlock.BulletType);
					renderer.WriteRaw('"');
				}
				if (listBlock.OrderedStart != null && listBlock.OrderedStart != "1")
				{
					renderer.Write(" start=\"");
					renderer.WriteRaw(listBlock.OrderedStart);
					renderer.WriteRaw('"');
				}
				renderer.WriteAttributes(listBlock);
				renderer.WriteLine('>');
			}
			else
			{
				renderer.Write("<ul");
				renderer.WriteAttributes(listBlock);
				renderer.WriteLine('>');
			}
		}
		foreach (ListItemBlock item in listBlock)
		{
			bool implicitParagraph = renderer.ImplicitParagraph;
			renderer.ImplicitParagraph = !listBlock.IsLoose;
			renderer.EnsureLine();
			if (renderer.EnableHtmlForBlock)
			{
				renderer.Write("<li");
				renderer.WriteAttributes(item);
				renderer.WriteRaw('>');
			}
			renderer.WriteChildren(item);
			if (renderer.EnableHtmlForBlock)
			{
				renderer.WriteLine("</li>");
			}
			renderer.EnsureLine();
			renderer.ImplicitParagraph = implicitParagraph;
		}
		if (renderer.EnableHtmlForBlock)
		{
			renderer.WriteLine(listBlock.IsOrdered ? "</ol>" : "</ul>");
		}
		renderer.EnsureLine();
	}
}
