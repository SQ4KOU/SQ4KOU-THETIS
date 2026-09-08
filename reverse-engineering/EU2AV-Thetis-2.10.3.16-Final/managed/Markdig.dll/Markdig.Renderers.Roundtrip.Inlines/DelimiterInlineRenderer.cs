using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class DelimiterInlineRenderer : RoundtripObjectRenderer<DelimiterInline>
{
	protected override void Write(RoundtripRenderer renderer, DelimiterInline obj)
	{
		renderer.Write(obj.ToLiteral());
		renderer.WriteChildren(obj);
	}
}
