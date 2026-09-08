using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Markdig.Extensions.Hardlines;

public class SoftlineBreakAsHardlineExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		LineBreakInlineParser lineBreakInlineParser = pipeline.InlineParsers.Find<LineBreakInlineParser>();
		if (lineBreakInlineParser != null)
		{
			lineBreakInlineParser.EnableSoftAsHard = true;
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}
}
