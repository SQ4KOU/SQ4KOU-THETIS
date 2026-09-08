using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class EmphasisInlineRenderer : NormalizeObjectRenderer<EmphasisInline>
{
	protected override void Write(NormalizeRenderer renderer, EmphasisInline obj)
	{
		renderer.Write(obj.DelimiterChar, obj.DelimiterCount);
		renderer.WriteChildren(obj);
		renderer.Write(obj.DelimiterChar, obj.DelimiterCount);
	}
}
