using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Footnotes;

public class HtmlFootnoteGroupRenderer : HtmlObjectRenderer<FootnoteGroup>
{
	public string GroupClass { get; set; }

	public HtmlFootnoteGroupRenderer()
	{
		GroupClass = "footnotes";
	}

	protected override void Write(HtmlRenderer renderer, FootnoteGroup footnotes)
	{
		renderer.EnsureLine();
		renderer.WriteLine("<div class=\"" + GroupClass + "\">");
		renderer.WriteLine("<hr />");
		renderer.WriteLine("<ol>");
		for (int i = 0; i < footnotes.Count; i++)
		{
			Footnote footnote = (Footnote)footnotes[i];
			renderer.WriteLine($"<li id=\"fn:{footnote.Order}\">");
			renderer.WriteChildren(footnote);
			renderer.WriteLine("</li>");
		}
		renderer.WriteLine("</ol>");
		renderer.WriteLine("</div>");
	}
}
