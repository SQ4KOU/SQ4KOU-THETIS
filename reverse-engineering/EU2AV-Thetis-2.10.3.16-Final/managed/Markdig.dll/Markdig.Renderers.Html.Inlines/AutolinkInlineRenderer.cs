using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class AutolinkInlineRenderer : HtmlObjectRenderer<AutolinkInline>
{
	public string? Rel { get; set; }

	protected override void Write(HtmlRenderer renderer, AutolinkInline obj)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write(obj.IsEmail ? "<a href=\"mailto:" : "<a href=\"");
			renderer.WriteEscapeUrl(obj.Url);
			renderer.WriteRaw('"');
			renderer.WriteAttributes(obj);
			if (!obj.IsEmail && !string.IsNullOrWhiteSpace(Rel))
			{
				renderer.WriteRaw(" rel=\"");
				renderer.WriteRaw(Rel);
				renderer.WriteRaw('"');
			}
			renderer.WriteRaw('>');
		}
		renderer.WriteEscape(obj.Url);
		if (renderer.EnableHtmlForInline)
		{
			renderer.WriteRaw("</a>");
		}
	}
}
