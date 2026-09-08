using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class NormalizeHtmlEntityInlineRenderer : NormalizeObjectRenderer<HtmlEntityInline>
{
	protected override void Write(NormalizeRenderer renderer, HtmlEntityInline obj)
	{
		renderer.Write(obj.Original);
	}
}
