using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class LiteralInlineRenderer : RoundtripObjectRenderer<LiteralInline>
{
	protected override void Write(RoundtripRenderer renderer, LiteralInline obj)
	{
		if (obj.IsFirstCharacterEscaped && obj.Content.Length > 0 && obj.Content[obj.Content.Start].IsAsciiPunctuation())
		{
			renderer.Write('\\');
		}
		renderer.Write(ref obj.Content);
	}
}
