using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip;

public class QuoteBlockRenderer : RoundtripObjectRenderer<QuoteBlock>
{
	protected override void Write(RoundtripRenderer renderer, QuoteBlock quoteBlock)
	{
		renderer.RenderLinesBefore(quoteBlock);
		renderer.Write(quoteBlock.TriviaBefore);
		string[] array = new string[quoteBlock.QuoteLines.Count];
		for (int i = 0; i < quoteBlock.QuoteLines.Count; i++)
		{
			QuoteBlockLine quoteBlockLine = quoteBlock.QuoteLines[i];
			string text = quoteBlockLine.TriviaBefore.ToString();
			string text2 = (quoteBlockLine.QuoteChar ? ">" : "");
			string text3 = (quoteBlockLine.HasSpaceAfterQuoteChar ? " " : "");
			string text4 = quoteBlockLine.TriviaAfter.ToString();
			array[i] = text + text2 + text3 + text4;
		}
		bool flag = false;
		if (quoteBlock.Count == 0)
		{
			flag = true;
			foreach (QuoteBlockLine quoteLine in quoteBlock.QuoteLines)
			{
				ParagraphBlock paragraphBlock = new ParagraphBlock
				{
					NewLine = quoteLine.NewLine
				};
				LineBreakInline child = new LineBreakInline
				{
					NewLine = quoteLine.NewLine
				};
				ContainerInline containerInline = new ContainerInline();
				containerInline.AppendChild(child);
				paragraphBlock.Inline = containerInline;
				quoteBlock.Add(paragraphBlock);
			}
		}
		renderer.PushIndent(array);
		renderer.WriteChildren(quoteBlock);
		renderer.PopIndent();
		if (!flag)
		{
			renderer.RenderLinesAfter(quoteBlock);
		}
	}
}
