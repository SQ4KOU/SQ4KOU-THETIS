using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace Markdig.Extensions.ReferralLinks;

public class ReferralLinksExtension : IMarkdownExtension
{
	public List<string> Rels { get; }

	public ReferralLinksExtension(string[] rels)
	{
		Rels = rels?.ToList() ?? throw new ArgumentNullException("rels");
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		string rel = string.Join(" ", Rels.Where((string r) => !string.IsNullOrEmpty(r)));
		LinkInlineRenderer linkInlineRenderer = renderer.ObjectRenderers.Find<LinkInlineRenderer>();
		if (linkInlineRenderer != null)
		{
			linkInlineRenderer.Rel = rel;
		}
		AutolinkInlineRenderer autolinkInlineRenderer = renderer.ObjectRenderers.Find<AutolinkInlineRenderer>();
		if (autolinkInlineRenderer != null)
		{
			autolinkInlineRenderer.Rel = rel;
		}
	}
}
