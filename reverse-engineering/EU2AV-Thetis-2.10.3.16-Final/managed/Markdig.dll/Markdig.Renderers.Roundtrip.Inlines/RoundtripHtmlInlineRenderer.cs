using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class RoundtripHtmlInlineRenderer : RoundtripObjectRenderer<HtmlInline>
{
	protected override void Write(RoundtripRenderer renderer, HtmlInline obj)
	{
		renderer.Write(obj.Tag);
	}
}
