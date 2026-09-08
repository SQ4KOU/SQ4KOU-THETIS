using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class LinkInlineRenderer : HtmlObjectRenderer<LinkInline>
{
	public string? Rel { get; set; }

	protected override void Write(HtmlRenderer renderer, LinkInline link)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write(link.IsImage ? "<img src=\"" : "<a href=\"");
			renderer.WriteEscapeUrl((link.GetDynamicUrl != null) ? (link.GetDynamicUrl() ?? link.Url) : link.Url);
			renderer.WriteRaw('"');
			renderer.WriteAttributes(link);
		}
		if (link.IsImage)
		{
			if (renderer.EnableHtmlForInline)
			{
				renderer.WriteRaw(" alt=\"");
			}
			bool enableHtmlForInline = renderer.EnableHtmlForInline;
			renderer.EnableHtmlForInline = false;
			renderer.WriteChildren(link);
			renderer.EnableHtmlForInline = enableHtmlForInline;
			if (renderer.EnableHtmlForInline)
			{
				renderer.WriteRaw('"');
			}
		}
		if (renderer.EnableHtmlForInline && !string.IsNullOrEmpty(link.Title))
		{
			renderer.WriteRaw(" title=\"");
			renderer.WriteEscape(link.Title);
			renderer.WriteRaw('"');
		}
		if (link.IsImage)
		{
			if (renderer.EnableHtmlForInline)
			{
				renderer.WriteRaw(" />");
			}
			return;
		}
		if (renderer.EnableHtmlForInline)
		{
			if (!string.IsNullOrWhiteSpace(Rel))
			{
				renderer.WriteRaw(" rel=\"");
				renderer.WriteRaw(Rel);
				renderer.WriteRaw('"');
			}
			renderer.WriteRaw('>');
		}
		renderer.WriteChildren(link);
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("</a>");
		}
	}
}
