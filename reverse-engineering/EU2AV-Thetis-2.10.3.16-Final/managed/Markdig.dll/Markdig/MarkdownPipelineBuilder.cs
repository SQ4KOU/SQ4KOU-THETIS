using System.IO;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;

namespace Markdig;

public class MarkdownPipelineBuilder
{
	private MarkdownPipeline? _pipeline;

	public OrderedList<BlockParser> BlockParsers { get; private set; }

	public OrderedList<InlineParser> InlineParsers { get; private set; }

	public OrderedList<IMarkdownExtension> Extensions { get; }

	public bool PreciseSourceLocation { get; set; }

	public TextWriter? DebugLog { get; set; }

	public bool TrackTrivia { get; set; }

	internal ProcessDocumentDelegate? GetDocumentProcessed => DocumentProcessed;

	public event ProcessDocumentDelegate? DocumentProcessed;

	public MarkdownPipelineBuilder()
	{
		BlockParsers = new OrderedList<BlockParser>
		{
			new ThematicBreakParser(),
			new HeadingBlockParser(),
			new QuoteBlockParser(),
			new ListBlockParser(),
			new HtmlBlockParser(),
			new FencedCodeBlockParser(),
			new IndentedCodeBlockParser(),
			new ParagraphBlockParser()
		};
		InlineParsers = new OrderedList<InlineParser>
		{
			new HtmlEntityParser(),
			new LinkInlineParser(),
			new EscapeInlineParser(),
			new EmphasisInlineParser(),
			new CodeInlineParser(),
			new AutolinkInlineParser(),
			new LineBreakInlineParser()
		};
		Extensions = new OrderedList<IMarkdownExtension>();
	}

	public MarkdownPipeline Build()
	{
		if (_pipeline != null)
		{
			return _pipeline;
		}
		foreach (IMarkdownExtension extension in Extensions)
		{
			if (extension == null)
			{
				ThrowHelper.InvalidOperationException("An extension cannot be null");
			}
			extension.Setup(this);
		}
		_pipeline = new MarkdownPipeline(new OrderedList<IMarkdownExtension>(Extensions), new BlockParserList(BlockParsers), new InlineParserList(InlineParsers), DebugLog, GetDocumentProcessed)
		{
			PreciseSourceLocation = PreciseSourceLocation,
			TrackTrivia = TrackTrivia
		};
		return _pipeline;
	}
}
