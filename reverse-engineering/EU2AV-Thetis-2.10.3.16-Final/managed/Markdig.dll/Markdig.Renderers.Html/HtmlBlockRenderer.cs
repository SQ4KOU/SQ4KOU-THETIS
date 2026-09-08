using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class HtmlBlockRenderer : HtmlObjectRenderer<HtmlBlock>
{
	protected override void Write(HtmlRenderer renderer, HtmlBlock obj)
	{
		renderer.WriteLeafRawLines(obj, writeEndOfLines: true, escape: false);
	}
}
