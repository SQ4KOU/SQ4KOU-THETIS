using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class RoundtripHtmlEntityInlineRenderer : RoundtripObjectRenderer<HtmlEntityInline>
{
	protected override void Write(RoundtripRenderer renderer, HtmlEntityInline obj)
	{
		renderer.Write(obj.Original);
	}
}
