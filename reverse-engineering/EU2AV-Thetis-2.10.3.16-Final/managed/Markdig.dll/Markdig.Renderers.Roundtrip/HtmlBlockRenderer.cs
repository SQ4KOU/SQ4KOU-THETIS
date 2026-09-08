using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class HtmlBlockRenderer : RoundtripObjectRenderer<HtmlBlock>
{
	protected override void Write(RoundtripRenderer renderer, HtmlBlock obj)
	{
		renderer.RenderLinesBefore(obj);
		renderer.WriteLeafRawLines(obj);
		renderer.RenderLinesAfter(obj);
	}
}
