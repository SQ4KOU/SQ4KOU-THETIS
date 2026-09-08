using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class AutolinkInlineRenderer : NormalizeObjectRenderer<AutolinkInline>
{
	protected override void Write(NormalizeRenderer renderer, AutolinkInline obj)
	{
		renderer.Write('<').Write(obj.Url).Write('>');
	}
}
