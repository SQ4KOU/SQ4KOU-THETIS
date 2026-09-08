using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Citations;

public class CitationExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		EmphasisInlineParser emphasisInlineParser = pipeline.InlineParsers.FindExact<EmphasisInlineParser>();
		if (emphasisInlineParser != null && !emphasisInlineParser.HasEmphasisChar('"'))
		{
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor('"', 2, 2, enableWithinWord: false));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (!(renderer is HtmlRenderer))
		{
			return;
		}
		EmphasisInlineRenderer emphasisInlineRenderer = renderer.ObjectRenderers.FindExact<EmphasisInlineRenderer>();
		if (emphasisInlineRenderer != null)
		{
			EmphasisInlineRenderer.GetTagDelegate previousTag = emphasisInlineRenderer.GetTag;
			emphasisInlineRenderer.GetTag = (EmphasisInline inline) => GetTag(inline) ?? previousTag(inline);
		}
	}

	private static string? GetTag(EmphasisInline emphasisInline)
	{
		if (emphasisInline.DelimiterCount != 2 || emphasisInline.DelimiterChar != '"')
		{
			return null;
		}
		return "cite";
	}
}
