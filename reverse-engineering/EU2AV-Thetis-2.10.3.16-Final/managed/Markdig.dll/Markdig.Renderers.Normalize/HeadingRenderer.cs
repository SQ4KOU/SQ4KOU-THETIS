using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class HeadingRenderer : NormalizeObjectRenderer<HeadingBlock>
{
	private static readonly string[] HeadingTexts = new string[6] { "#", "##", "###", "####", "#####", "######" };

	protected override void Write(NormalizeRenderer renderer, HeadingBlock obj)
	{
		int level = obj.Level;
		if (level > 0 && level <= 6)
		{
			renderer.Write(HeadingTexts[obj.Level - 1]);
		}
		else
		{
			renderer.Write('#', obj.Level);
		}
		renderer.Write(' ');
		renderer.WriteLeafInline(obj);
		renderer.FinishBlock(renderer.Options.EmptyLineAfterHeading);
	}
}
