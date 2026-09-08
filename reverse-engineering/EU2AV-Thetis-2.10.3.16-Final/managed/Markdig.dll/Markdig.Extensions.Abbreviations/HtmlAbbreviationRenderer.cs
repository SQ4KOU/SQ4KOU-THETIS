using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Abbreviations;

public class HtmlAbbreviationRenderer : HtmlObjectRenderer<AbbreviationInline>
{
	protected override void Write(HtmlRenderer renderer, AbbreviationInline obj)
	{
		Abbreviation abbreviation = obj.Abbreviation;
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("<abbr").WriteAttributes(obj).Write(" title=\"")
				.WriteEscape(ref abbreviation.Text)
				.Write("\">");
		}
		renderer.Write(abbreviation.Label);
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("</abbr>");
		}
	}
}
