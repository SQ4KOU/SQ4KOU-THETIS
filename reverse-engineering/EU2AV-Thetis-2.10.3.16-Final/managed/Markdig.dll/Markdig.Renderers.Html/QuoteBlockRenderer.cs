using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class QuoteBlockRenderer : HtmlObjectRenderer<QuoteBlock>
{
	protected override void Write(HtmlRenderer renderer, QuoteBlock obj)
	{
		renderer.EnsureLine();
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("<blockquote");
			renderer.WriteAttributes(obj);
			renderer.WriteLine('>');
		}
		bool implicitParagraph = renderer.ImplicitParagraph;
		renderer.ImplicitParagraph = false;
		renderer.WriteChildren(obj);
		renderer.ImplicitParagraph = implicitParagraph;
		if (renderer.EnableHtmlForBlock)
		{
			renderer.WriteLine("</blockquote>");
		}
		renderer.EnsureLine();
	}
}
