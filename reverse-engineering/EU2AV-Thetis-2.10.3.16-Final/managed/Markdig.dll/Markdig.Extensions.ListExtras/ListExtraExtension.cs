using Markdig.Parsers;
using Markdig.Renderers;

namespace Markdig.Extensions.ListExtras;

public class ListExtraExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.BlockParsers.Find<ListBlockParser>()?.ItemParsers.AddIfNotAlready<ListExtraItemParser>();
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}
}
