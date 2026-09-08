using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Markdig.Extensions.Tables;

public class PipeTableExtension : IMarkdownExtension
{
	public PipeTableOptions Options { get; }

	public PipeTableExtension(PipeTableOptions? options = null)
	{
		Options = options ?? new PipeTableOptions();
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.PreciseSourceLocation = true;
		if (!pipeline.BlockParsers.Contains<PipeTableBlockParser>())
		{
			pipeline.BlockParsers.Insert(0, new PipeTableBlockParser());
		}
		LineBreakInlineParser lineBreakParser = pipeline.InlineParsers.FindExact<LineBreakInlineParser>();
		if (!pipeline.InlineParsers.Contains<PipeTableParser>())
		{
			pipeline.InlineParsers.InsertAfter<EmphasisInlineParser>(new PipeTableParser(lineBreakParser, Options));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<HtmlTableRenderer>())
		{
			htmlRenderer.ObjectRenderers.Add(new HtmlTableRenderer());
		}
	}
}
