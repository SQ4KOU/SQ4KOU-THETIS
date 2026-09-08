using Markdig.Renderers;

namespace Markdig.Extensions.TextRenderer;

public class ConfigureNewLineExtension : IMarkdownExtension
{
	private readonly string newLine;

	public ConfigureNewLineExtension(string newLine)
	{
		this.newLine = newLine;
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is TextRendererBase textRendererBase)
		{
			textRendererBase.Writer.NewLine = newLine;
		}
	}
}
