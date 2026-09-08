using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Figures;

public class HtmlFigureRenderer : HtmlObjectRenderer<Figure>
{
	protected override void Write(HtmlRenderer renderer, Figure obj)
	{
		renderer.EnsureLine();
		renderer.Write("<figure").WriteAttributes(obj).WriteLine(">");
		renderer.WriteChildren(obj);
		renderer.WriteLine("</figure>");
	}
}
