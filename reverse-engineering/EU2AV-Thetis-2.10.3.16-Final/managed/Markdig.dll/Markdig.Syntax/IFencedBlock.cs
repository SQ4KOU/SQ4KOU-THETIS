using Markdig.Helpers;

namespace Markdig.Syntax;

public interface IFencedBlock : IBlock, IMarkdownObject
{
	char FencedChar { get; set; }

	int OpeningFencedCharCount { get; set; }

	StringSlice TriviaAfterFencedChar { get; set; }

	string? Info { get; set; }

	StringSlice UnescapedInfo { get; set; }

	StringSlice TriviaAfterInfo { get; set; }

	string? Arguments { get; set; }

	StringSlice UnescapedArguments { get; set; }

	StringSlice TriviaAfterArguments { get; set; }

	NewLine InfoNewLine { get; set; }

	StringSlice TriviaBeforeClosingFence { get; set; }

	int ClosingFencedCharCount { get; set; }

	NewLine NewLine { get; set; }
}
