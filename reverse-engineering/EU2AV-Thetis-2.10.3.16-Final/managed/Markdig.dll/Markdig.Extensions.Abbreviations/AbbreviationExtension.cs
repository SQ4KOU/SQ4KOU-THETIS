using Markdig.Renderers;

namespace Markdig.Extensions.Abbreviations;

public class AbbreviationExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.BlockParsers.AddIfNotAlready<AbbreviationParser>();
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<HtmlAbbreviationRenderer>())
		{
			htmlRenderer.ObjectRenderers.Insert(0, new HtmlAbbreviationRenderer());
		}
	}
}
