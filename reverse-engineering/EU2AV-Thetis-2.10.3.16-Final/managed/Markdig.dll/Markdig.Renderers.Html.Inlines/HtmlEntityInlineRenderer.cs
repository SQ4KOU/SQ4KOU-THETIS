using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class HtmlEntityInlineRenderer : HtmlObjectRenderer<HtmlEntityInline>
{
	protected override void Write(HtmlRenderer renderer, HtmlEntityInline obj)
	{
		if (renderer.EnableHtmlEscape)
		{
			renderer.WriteEscape(obj.Transcoded);
		}
		else
		{
			renderer.Write(obj.Transcoded);
		}
	}
}
