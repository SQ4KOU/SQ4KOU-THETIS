using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class CodeInlineRenderer : HtmlObjectRenderer<CodeInline>
{
	protected override void Write(HtmlRenderer renderer, CodeInline obj)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("<code");
			renderer.WriteAttributes(obj);
			renderer.WriteRaw('>');
		}
		if (renderer.EnableHtmlEscape)
		{
			renderer.WriteEscape(obj.ContentSpan);
		}
		else
		{
			renderer.Write(obj.ContentSpan);
		}
		if (renderer.EnableHtmlForInline)
		{
			renderer.WriteRaw("</code>");
		}
	}
}
