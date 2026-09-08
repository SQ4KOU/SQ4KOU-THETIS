using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class LiteralInlineRenderer : NormalizeObjectRenderer<LiteralInline>
{
	protected override void Write(NormalizeRenderer renderer, LiteralInline obj)
	{
		if (obj.IsFirstCharacterEscaped && obj.Content.Length > 0 && obj.Content[obj.Content.Start].IsAsciiPunctuation())
		{
			renderer.Write('\\');
		}
		renderer.Write(ref obj.Content);
	}
}
