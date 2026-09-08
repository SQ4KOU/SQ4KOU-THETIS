using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class ThematicBreakRenderer : NormalizeObjectRenderer<ThematicBreakBlock>
{
	protected override void Write(NormalizeRenderer renderer, ThematicBreakBlock obj)
	{
		renderer.WriteLine(new string(obj.ThematicChar, obj.ThematicCharCount));
		renderer.FinishBlock(renderer.Options.EmptyLineAfterThematicBreak);
	}
}
