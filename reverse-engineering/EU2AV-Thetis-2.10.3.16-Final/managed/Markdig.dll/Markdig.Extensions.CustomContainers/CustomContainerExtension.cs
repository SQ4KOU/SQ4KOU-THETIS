using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Markdig.Extensions.CustomContainers;

public class CustomContainerExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<CustomContainerParser>())
		{
			pipeline.BlockParsers.Insert(0, new CustomContainerParser());
		}
		EmphasisInlineParser emphasisInlineParser = pipeline.InlineParsers.Find<EmphasisInlineParser>();
		if (emphasisInlineParser != null && !emphasisInlineParser.HasEmphasisChar(':'))
		{
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor(':', 2, 2, enableWithinWord: true));
			emphasisInlineParser.TryCreateEmphasisInlineList.Add((char emphasisChar, int delimiterCount) => (delimiterCount == 2 && emphasisChar == ':') ? new CustomContainerInline
			{
				DelimiterChar = ':',
				DelimiterCount = 2
			} : null);
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			if (!htmlRenderer.ObjectRenderers.Contains<HtmlCustomContainerRenderer>())
			{
				htmlRenderer.ObjectRenderers.Insert(0, new HtmlCustomContainerRenderer());
			}
			if (!htmlRenderer.ObjectRenderers.Contains<HtmlCustomContainerInlineRenderer>())
			{
				htmlRenderer.ObjectRenderers.Insert(0, new HtmlCustomContainerInlineRenderer());
			}
		}
	}
}
