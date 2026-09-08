using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class HtmlInlineRenderer : HtmlObjectRenderer<HtmlInline>
{
	protected override void Write(HtmlRenderer renderer, HtmlInline obj)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write(obj.Tag);
		}
	}
}
