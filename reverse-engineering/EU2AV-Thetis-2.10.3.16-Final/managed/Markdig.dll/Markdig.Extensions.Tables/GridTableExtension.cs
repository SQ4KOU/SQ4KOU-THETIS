using Markdig.Renderers;

namespace Markdig.Extensions.Tables;

public class GridTableExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<GridTableParser>())
		{
			pipeline.BlockParsers.Insert(0, new GridTableParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<HtmlTableRenderer>())
		{
			htmlRenderer.ObjectRenderers.Add(new HtmlTableRenderer());
		}
	}
}
