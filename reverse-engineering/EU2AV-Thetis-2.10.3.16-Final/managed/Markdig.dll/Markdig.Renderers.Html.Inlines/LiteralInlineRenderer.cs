using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class LiteralInlineRenderer : HtmlObjectRenderer<LiteralInline>
{
	protected override void Write(HtmlRenderer renderer, LiteralInline obj)
	{
		if (renderer.EnableHtmlEscape)
		{
			renderer.WriteEscape(ref obj.Content);
		}
		else
		{
			renderer.Write(ref obj.Content);
		}
	}
}
