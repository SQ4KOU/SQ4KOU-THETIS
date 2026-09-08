using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.CustomContainers;

public class HtmlCustomContainerRenderer : HtmlObjectRenderer<CustomContainer>
{
	protected override void Write(HtmlRenderer renderer, CustomContainer obj)
	{
		renderer.EnsureLine();
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("<div").WriteAttributes(obj).Write('>');
		}
		renderer.WriteChildren(obj);
		if (renderer.EnableHtmlForBlock)
		{
			renderer.WriteLine("</div>");
		}
	}
}
