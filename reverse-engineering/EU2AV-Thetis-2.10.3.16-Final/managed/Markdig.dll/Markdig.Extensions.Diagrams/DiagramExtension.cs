using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Diagrams;

public class DiagramExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			CodeBlockRenderer? codeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
			codeBlockRenderer.BlockMapping["mermaid"] = "pre";
			codeBlockRenderer.BlocksAsDiv.Add("nomnoml");
		}
	}
}
