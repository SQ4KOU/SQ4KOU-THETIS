using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Mathematics;

public class HtmlMathInlineRenderer : HtmlObjectRenderer<MathInline>
{
	protected override void Write(HtmlRenderer renderer, MathInline obj)
	{
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("<span").WriteAttributes(obj).Write(">\\(");
		}
		if (renderer.EnableHtmlEscape)
		{
			renderer.WriteEscape(ref obj.Content);
		}
		else
		{
			renderer.Write(ref obj.Content);
		}
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("\\)</span>");
		}
	}
}
