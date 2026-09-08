using Markdig.Renderers;

namespace Markdig.Extensions.DefinitionLists;

public class DefinitionListExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<DefinitionListParser>())
		{
			pipeline.BlockParsers.Insert(0, new DefinitionListParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<HtmlDefinitionListRenderer>())
		{
			htmlRenderer.ObjectRenderers.Insert(0, new HtmlDefinitionListRenderer());
		}
	}
}
