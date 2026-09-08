using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class DelimiterInlineRenderer : NormalizeObjectRenderer<DelimiterInline>
{
	protected override void Write(NormalizeRenderer renderer, DelimiterInline obj)
	{
		renderer.Write(obj.ToLiteral());
		renderer.WriteChildren(obj);
	}
}
