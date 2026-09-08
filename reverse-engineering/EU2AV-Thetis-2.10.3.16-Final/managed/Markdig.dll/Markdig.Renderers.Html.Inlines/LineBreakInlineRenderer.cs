using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class LineBreakInlineRenderer : HtmlObjectRenderer<LineBreakInline>
{
	public bool RenderAsHardlineBreak { get; set; }

	protected override void Write(HtmlRenderer renderer, LineBreakInline obj)
	{
		if (!renderer.IsLastInContainer)
		{
			if (renderer.EnableHtmlForInline && (obj.IsHard || RenderAsHardlineBreak))
			{
				renderer.WriteLine("<br />");
			}
			renderer.EnsureLine();
		}
	}
}
