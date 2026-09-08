using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class DelimiterInlineRenderer : HtmlObjectRenderer<DelimiterInline>
{
	protected override void Write(HtmlRenderer renderer, DelimiterInline obj)
	{
		renderer.WriteEscape(obj.ToLiteral());
		renderer.WriteChildren(obj);
	}
}
