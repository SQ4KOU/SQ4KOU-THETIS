using Markdig.Renderers;

namespace Markdig;

public interface IMarkdownExtension
{
	void Setup(MarkdownPipelineBuilder pipeline);

	void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer);
}
