using System;
using Markdig.Helpers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Alerts;

public class AlertExtension : IMarkdownExtension
{
	public Action<HtmlRenderer, StringSlice>? RenderKind { get; set; }

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (pipeline.InlineParsers.Find<AlertInlineParser>() == null)
		{
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new AlertInlineParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer.ObjectRenderers.FindExact<AlertBlockRenderer>() == null)
		{
			renderer.ObjectRenderers.InsertBefore<QuoteBlockRenderer>(new AlertBlockRenderer
			{
				RenderKind = (RenderKind ?? new Action<HtmlRenderer, StringSlice>(AlertBlockRenderer.DefaultRenderKind))
			});
		}
	}
}
