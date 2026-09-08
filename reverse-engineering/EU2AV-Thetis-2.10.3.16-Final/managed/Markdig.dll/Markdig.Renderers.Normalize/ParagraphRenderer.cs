using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class ParagraphRenderer : NormalizeObjectRenderer<ParagraphBlock>
{
	protected override void Write(NormalizeRenderer renderer, ParagraphBlock obj)
	{
		renderer.WriteLeafInline(obj);
		renderer.FinishBlock(!renderer.CompactParagraph);
	}
}
