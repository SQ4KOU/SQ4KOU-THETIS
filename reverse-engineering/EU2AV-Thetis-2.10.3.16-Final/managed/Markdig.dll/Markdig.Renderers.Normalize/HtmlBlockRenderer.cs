using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class HtmlBlockRenderer : NormalizeObjectRenderer<HtmlBlock>
{
	protected override void Write(NormalizeRenderer renderer, HtmlBlock obj)
	{
		renderer.WriteLeafRawLines(obj, writeEndOfLines: true);
	}
}
