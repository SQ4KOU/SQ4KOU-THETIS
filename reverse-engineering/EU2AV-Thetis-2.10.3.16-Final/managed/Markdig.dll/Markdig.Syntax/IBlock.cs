using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public interface IBlock : IMarkdownObject
{
	int Column { get; set; }

	int Line { get; set; }

	ContainerBlock? Parent { get; }

	BlockParser? Parser { get; }

	bool IsOpen { get; set; }

	bool IsBreakable { get; set; }

	bool RemoveAfterProcessInlines { get; set; }

	StringSlice TriviaBefore { get; set; }

	StringSlice TriviaAfter { get; set; }

	event ProcessInlineDelegate? ProcessInlinesBegin;

	event ProcessInlineDelegate? ProcessInlinesEnd;
}
