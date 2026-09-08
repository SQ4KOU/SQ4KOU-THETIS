using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;

namespace Markdig.Extensions.JiraLinks;

public class JiraLinkExtension : IMarkdownExtension
{
	private readonly JiraLinkOptions _options;

	public JiraLinkExtension(JiraLinkOptions options)
	{
		_options = options;
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<JiraLinkInlineParser>())
		{
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new JiraLinkInlineParser(_options));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is NormalizeRenderer normalizeRenderer && !normalizeRenderer.ObjectRenderers.Contains<NormalizeJiraLinksRenderer>())
		{
			normalizeRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new NormalizeJiraLinksRenderer());
		}
	}
}
