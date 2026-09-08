using Markdig.Renderers;

namespace Markdig.Extensions.Emoji;

public class EmojiExtension : IMarkdownExtension
{
	public EmojiMapping EmojiMapping { get; }

	public EmojiExtension(EmojiMapping emojiMapping)
	{
		EmojiMapping = emojiMapping;
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<EmojiParser>())
		{
			pipeline.InlineParsers.Insert(0, new EmojiParser(EmojiMapping));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}
}
