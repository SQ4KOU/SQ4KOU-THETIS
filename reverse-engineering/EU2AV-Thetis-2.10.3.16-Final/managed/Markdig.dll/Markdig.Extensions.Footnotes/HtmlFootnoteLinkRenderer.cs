using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Footnotes;

public class HtmlFootnoteLinkRenderer : HtmlObjectRenderer<FootnoteLink>
{
	public string BackLinkString { get; set; }

	public string FootnoteLinkClass { get; set; }

	public string FootnoteBackLinkClass { get; set; }

	public HtmlFootnoteLinkRenderer()
	{
		BackLinkString = "&#8617;";
		FootnoteLinkClass = "footnote-ref";
		FootnoteBackLinkClass = "footnote-back-ref";
	}

	protected override void Write(HtmlRenderer renderer, FootnoteLink link)
	{
		int order = link.Footnote.Order;
		renderer.Write(link.IsBackLink ? $"<a href=\"#fnref:{link.Index}\" class=\"{FootnoteBackLinkClass}\">{BackLinkString}</a>" : $"<a id=\"fnref:{link.Index}\" href=\"#fn:{order}\" class=\"{FootnoteLinkClass}\"><sup>{order}</sup></a>");
	}
}
