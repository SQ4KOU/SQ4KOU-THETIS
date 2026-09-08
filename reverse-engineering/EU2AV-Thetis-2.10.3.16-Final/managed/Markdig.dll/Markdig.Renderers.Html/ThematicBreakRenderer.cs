using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class ThematicBreakRenderer : HtmlObjectRenderer<ThematicBreakBlock>
{
	protected override void Write(HtmlRenderer renderer, ThematicBreakBlock obj)
	{
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("<hr");
			renderer.WriteAttributes(obj);
			renderer.WriteLine(" />");
		}
	}
}
