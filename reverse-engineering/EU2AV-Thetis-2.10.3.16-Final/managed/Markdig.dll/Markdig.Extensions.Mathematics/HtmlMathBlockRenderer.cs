using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Mathematics;

public class HtmlMathBlockRenderer : HtmlObjectRenderer<MathBlock>
{
	protected override void Write(HtmlRenderer renderer, MathBlock obj)
	{
		renderer.EnsureLine();
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("<div").WriteAttributes(obj).WriteLine(">");
			renderer.WriteLine("\\[");
		}
		renderer.WriteLeafRawLines(obj, writeEndOfLines: true, renderer.EnableHtmlEscape);
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("\\]");
			renderer.WriteLine("</div>");
		}
	}
}
