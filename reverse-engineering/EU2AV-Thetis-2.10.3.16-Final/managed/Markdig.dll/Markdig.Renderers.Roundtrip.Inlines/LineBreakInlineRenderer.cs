using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class LineBreakInlineRenderer : RoundtripObjectRenderer<LineBreakInline>
{
	protected override void Write(RoundtripRenderer renderer, LineBreakInline obj)
	{
		if (obj.IsHard && obj.IsBackslash)
		{
			renderer.Write("\\");
		}
		renderer.WriteLine(obj.NewLine);
	}
}
