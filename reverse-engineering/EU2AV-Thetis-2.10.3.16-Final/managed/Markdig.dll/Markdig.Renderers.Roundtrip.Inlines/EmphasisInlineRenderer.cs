using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class EmphasisInlineRenderer : RoundtripObjectRenderer<EmphasisInline>
{
	protected override void Write(RoundtripRenderer renderer, EmphasisInline obj)
	{
		renderer.Write(obj.DelimiterChar, obj.DelimiterCount);
		renderer.WriteChildren(obj);
		renderer.Write(obj.DelimiterChar, obj.DelimiterCount);
	}
}
