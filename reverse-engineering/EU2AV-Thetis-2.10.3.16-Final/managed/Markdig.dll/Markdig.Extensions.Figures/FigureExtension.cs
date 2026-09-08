using Markdig.Extensions.Footers;
using Markdig.Renderers;

namespace Markdig.Extensions.Figures;

public class FigureExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<FigureBlockParser>())
		{
			if (pipeline.BlockParsers.Contains<FooterBlockParser>())
			{
				pipeline.BlockParsers.InsertBefore<FooterBlockParser>(new FigureBlockParser());
			}
			else
			{
				pipeline.BlockParsers.Insert(0, new FigureBlockParser());
			}
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			htmlRenderer.ObjectRenderers.AddIfNotAlready<HtmlFigureRenderer>();
			htmlRenderer.ObjectRenderers.AddIfNotAlready<HtmlFigureCaptionRenderer>();
		}
	}
}
