using Markdig.Syntax.Inlines;

namespace Markdig.Parsers;

public interface IPostInlineProcessor
{
	bool PostProcess(InlineProcessor state, Inline? root, Inline? lastChild, int postInlineProcessorIndex, bool isFinalProcessing);
}
