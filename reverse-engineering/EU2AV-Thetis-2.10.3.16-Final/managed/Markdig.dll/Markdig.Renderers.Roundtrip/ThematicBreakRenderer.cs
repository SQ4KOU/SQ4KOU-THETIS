using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class ThematicBreakRenderer : RoundtripObjectRenderer<ThematicBreakBlock>
{
	protected override void Write(RoundtripRenderer renderer, ThematicBreakBlock obj)
	{
		renderer.RenderLinesBefore(obj);
		renderer.Write(obj.Content);
		renderer.WriteLine(obj.NewLine);
		renderer.RenderLinesAfter(obj);
	}
}
