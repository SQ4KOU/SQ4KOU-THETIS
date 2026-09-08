using Markdig.Renderers;

namespace Markdig.Extensions.Footnotes;

public class FootnoteExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<FootnoteParser>())
		{
			pipeline.BlockParsers.Insert(0, new FootnoteParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlFootnoteGroupRenderer());
			htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlFootnoteLinkRenderer());
		}
	}
}
