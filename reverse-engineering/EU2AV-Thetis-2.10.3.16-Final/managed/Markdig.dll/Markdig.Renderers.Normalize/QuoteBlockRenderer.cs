using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class QuoteBlockRenderer : NormalizeObjectRenderer<QuoteBlock>
{
	protected override void Write(NormalizeRenderer renderer, QuoteBlock obj)
	{
		string indent = (renderer.Options.SpaceAfterQuoteBlock ? (obj.QuoteChar + " ") : obj.QuoteChar.ToString());
		renderer.PushIndent(indent);
		renderer.WriteChildren(obj);
		renderer.PopIndent();
		renderer.FinishBlock(emptyLine: true);
	}
}
