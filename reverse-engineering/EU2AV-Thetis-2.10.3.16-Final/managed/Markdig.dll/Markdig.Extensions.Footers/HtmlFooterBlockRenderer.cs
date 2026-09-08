using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Footers;

public class HtmlFooterBlockRenderer : HtmlObjectRenderer<FooterBlock>
{
	protected override void Write(HtmlRenderer renderer, FooterBlock footer)
	{
		renderer.EnsureLine();
		renderer.Write("<footer").WriteAttributes(footer).Write(">");
		bool implicitParagraph = renderer.ImplicitParagraph;
		renderer.ImplicitParagraph = true;
		renderer.WriteChildren(footer);
		renderer.ImplicitParagraph = implicitParagraph;
		renderer.WriteLine("</footer>");
	}
}
