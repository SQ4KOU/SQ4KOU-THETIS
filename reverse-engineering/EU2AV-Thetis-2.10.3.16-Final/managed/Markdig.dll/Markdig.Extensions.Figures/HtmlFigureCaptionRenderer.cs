using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Figures;

public class HtmlFigureCaptionRenderer : HtmlObjectRenderer<FigureCaption>
{
	protected override void Write(HtmlRenderer renderer, FigureCaption obj)
	{
		renderer.EnsureLine();
		renderer.Write("<figcaption").WriteAttributes(obj).Write('>');
		renderer.WriteLeafInline(obj);
		renderer.WriteLine("</figcaption>");
	}
}
