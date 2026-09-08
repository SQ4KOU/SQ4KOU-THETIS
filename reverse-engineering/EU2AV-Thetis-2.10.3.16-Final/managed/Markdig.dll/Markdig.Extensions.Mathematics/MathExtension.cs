using Markdig.Renderers;

namespace Markdig.Extensions.Mathematics;

public class MathExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<MathInlineParser>())
		{
			pipeline.InlineParsers.Insert(0, new MathInlineParser());
		}
		if (!pipeline.BlockParsers.Contains<MathBlockParser>())
		{
			pipeline.BlockParsers.Insert(0, new MathBlockParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathInlineRenderer>())
			{
				htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathInlineRenderer());
			}
			if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathBlockRenderer>())
			{
				htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathBlockRenderer());
			}
		}
	}
}
