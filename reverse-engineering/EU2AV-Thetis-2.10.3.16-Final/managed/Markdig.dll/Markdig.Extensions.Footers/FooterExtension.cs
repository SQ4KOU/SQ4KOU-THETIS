using Markdig.Extensions.Figures;
using Markdig.Renderers;

namespace Markdig.Extensions.Footers;

public class FooterExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<FooterBlockParser>())
		{
			if (pipeline.BlockParsers.Contains<FigureBlockParser>())
			{
				pipeline.BlockParsers.InsertAfter<FigureBlockParser>(new FooterBlockParser());
			}
			else
			{
				pipeline.BlockParsers.Insert(0, new FooterBlockParser());
			}
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlFooterBlockRenderer());
		}
	}
}
