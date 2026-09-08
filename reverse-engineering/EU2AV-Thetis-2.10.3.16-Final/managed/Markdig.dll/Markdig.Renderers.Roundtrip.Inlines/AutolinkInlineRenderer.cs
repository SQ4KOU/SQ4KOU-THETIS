using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class AutolinkInlineRenderer : RoundtripObjectRenderer<AutolinkInline>
{
	protected override void Write(RoundtripRenderer renderer, AutolinkInline obj)
	{
		renderer.Write('<').Write(obj.Url).Write('>');
	}
}
