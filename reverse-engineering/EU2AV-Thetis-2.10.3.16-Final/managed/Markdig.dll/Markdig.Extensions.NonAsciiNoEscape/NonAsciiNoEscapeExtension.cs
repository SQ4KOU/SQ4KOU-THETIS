using Markdig.Renderers;

namespace Markdig.Extensions.NonAsciiNoEscape;

public class NonAsciiNoEscapeExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			htmlRenderer.UseNonAsciiNoEscape = true;
		}
	}
}
