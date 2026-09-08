using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class ParagraphRenderer : HtmlObjectRenderer<ParagraphBlock>
{
	protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
	{
		if (!renderer.ImplicitParagraph && renderer.EnableHtmlForBlock)
		{
			if (!renderer.IsFirstInContainer)
			{
				renderer.EnsureLine();
			}
			renderer.Write("<p");
			renderer.WriteAttributes(obj);
			renderer.WriteRaw('>');
		}
		renderer.WriteLeafInline(obj);
		if (!renderer.ImplicitParagraph)
		{
			if (renderer.EnableHtmlForBlock)
			{
				renderer.WriteLine("</p>");
			}
			renderer.EnsureLine();
		}
	}
}
