using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class NormalizeHtmlInlineRenderer : NormalizeObjectRenderer<HtmlInline>
{
	protected override void Write(NormalizeRenderer renderer, HtmlInline obj)
	{
		renderer.Write(obj.Tag);
	}
}
