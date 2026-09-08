using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.CustomContainers;

public class HtmlCustomContainerInlineRenderer : HtmlObjectRenderer<CustomContainerInline>
{
	protected override void Write(HtmlRenderer renderer, CustomContainerInline obj)
	{
		renderer.Write("<span").WriteAttributes(obj).Write('>');
		renderer.WriteChildren(obj);
		renderer.Write("</span>");
	}
}
