using System.Diagnostics;
using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

[DebuggerDisplay("renderer.Writer.ToString()")]
public class ParagraphRenderer : RoundtripObjectRenderer<ParagraphBlock>
{
	protected override void Write(RoundtripRenderer renderer, ParagraphBlock paragraph)
	{
		renderer.RenderLinesBefore(paragraph);
		renderer.Write(paragraph.TriviaBefore);
		renderer.WriteLeafInline(paragraph);
		renderer.RenderLinesAfter(paragraph);
	}
}
