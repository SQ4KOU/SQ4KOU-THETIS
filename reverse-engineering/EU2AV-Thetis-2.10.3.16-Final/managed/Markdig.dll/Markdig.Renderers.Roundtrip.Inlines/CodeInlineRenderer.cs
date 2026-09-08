using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class CodeInlineRenderer : RoundtripObjectRenderer<CodeInline>
{
	protected override void Write(RoundtripRenderer renderer, CodeInline obj)
	{
		renderer.Write(obj.Delimiter, obj.DelimiterCount);
		if (!obj.ContentSpan.IsEmpty)
		{
			renderer.Write(obj.ContentWithTrivia);
		}
		renderer.Write(obj.Delimiter, obj.DelimiterCount);
	}
}
