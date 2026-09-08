using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class HeadingRenderer : RoundtripObjectRenderer<HeadingBlock>
{
	private static readonly string[] HeadingTexts = new string[6] { "#", "##", "###", "####", "#####", "######" };

	protected override void Write(RoundtripRenderer renderer, HeadingBlock obj)
	{
		if (obj.IsSetext)
		{
			renderer.RenderLinesBefore(obj);
			char c = ((obj.Level == 1) ? '=' : '-');
			renderer.WriteLeafInline(obj);
			renderer.WriteLine(obj.SetextNewline);
			renderer.Write(obj.TriviaBefore);
			renderer.Write(c, obj.HeaderCharCount);
			renderer.WriteLine(obj.NewLine);
			renderer.Write(obj.TriviaAfter);
			renderer.RenderLinesAfter(obj);
			return;
		}
		renderer.RenderLinesBefore(obj);
		renderer.Write(obj.TriviaBefore);
		int level = obj.Level;
		if (level > 0 && level <= 6)
		{
			renderer.Write(HeadingTexts[obj.Level - 1]);
		}
		else
		{
			renderer.Write('#', obj.Level);
		}
		renderer.Write(obj.TriviaAfterAtxHeaderChar);
		renderer.WriteLeafInline(obj);
		renderer.Write(obj.TriviaAfter);
		renderer.WriteLine(obj.NewLine);
		renderer.RenderLinesAfter(obj);
	}
}
