using Markdig.Renderers;

namespace Markdig.Extensions.AutoLinks;

public class AutoLinkExtension(AutoLinkOptions? options) : IMarkdownExtension
{
	public readonly AutoLinkOptions Options = options ?? new AutoLinkOptions();

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<AutoLinkParser>())
		{
			pipeline.InlineParsers.Insert(0, new AutoLinkParser(Options));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}
}
