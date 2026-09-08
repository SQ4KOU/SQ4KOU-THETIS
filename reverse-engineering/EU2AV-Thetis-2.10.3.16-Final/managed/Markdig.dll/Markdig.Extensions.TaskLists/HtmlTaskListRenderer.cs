using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.TaskLists;

public class HtmlTaskListRenderer : HtmlObjectRenderer<TaskList>
{
	protected override void Write(HtmlRenderer renderer, TaskList obj)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("<input").WriteAttributes(obj).Write(" disabled=\"disabled\" type=\"checkbox\"");
			if (obj.Checked)
			{
				renderer.Write(" checked=\"checked\"");
			}
			renderer.Write(" />");
		}
		else
		{
			renderer.Write('[');
			renderer.Write(obj.Checked ? "x" : " ");
			renderer.Write(']');
		}
	}
}
