using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class LineBreakInlineRenderer : NormalizeObjectRenderer<LineBreakInline>
{
	public bool RenderAsHardlineBreak { get; set; }

	protected override void Write(NormalizeRenderer renderer, LineBreakInline obj)
	{
		if (obj.IsHard)
		{
			renderer.Write(obj.IsBackslash ? "\\" : "  ");
		}
		renderer.WriteLine();
	}
}
