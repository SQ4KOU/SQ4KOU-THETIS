using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public class FencedCodeBlock : CodeBlock, IFencedBlock, IBlock, IMarkdownObject
{
	private sealed class TriviaProperties
	{
		public StringSlice TriviaAfterFencedChar;

		public StringSlice UnescapedInfo;

		public StringSlice TriviaAfterInfo;

		public StringSlice UnescapedArguments;

		public StringSlice TriviaAfterArguments;

		public NewLine InfoNewLine;

		public StringSlice TriviaBeforeClosingFence;
	}

	private TriviaProperties? _trivia => TryGetDerivedTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetDerivedTrivia<TriviaProperties>();

	public int IndentCount { get; set; }

	public char FencedChar { get; set; }

	public int OpeningFencedCharCount { get; set; }

	public StringSlice TriviaAfterFencedChar
	{
		get
		{
			return _trivia?.TriviaAfterFencedChar ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterFencedChar = value;
		}
	}

	public string? Info { get; set; }

	public StringSlice UnescapedInfo
	{
		get
		{
			return _trivia?.UnescapedInfo ?? StringSlice.Empty;
		}
		set
		{
			Trivia.UnescapedInfo = value;
		}
	}

	public StringSlice TriviaAfterInfo
	{
		get
		{
			return _trivia?.TriviaAfterInfo ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterInfo = value;
		}
	}

	public string? Arguments { get; set; }

	public StringSlice UnescapedArguments
	{
		get
		{
			return _trivia?.UnescapedArguments ?? StringSlice.Empty;
		}
		set
		{
			Trivia.UnescapedArguments = value;
		}
	}

	public StringSlice TriviaAfterArguments
	{
		get
		{
			return _trivia?.TriviaAfterArguments ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterArguments = value;
		}
	}

	public NewLine InfoNewLine
	{
		get
		{
			return _trivia?.InfoNewLine ?? NewLine.None;
		}
		set
		{
			Trivia.InfoNewLine = value;
		}
	}

	public StringSlice TriviaBeforeClosingFence
	{
		get
		{
			return _trivia?.TriviaBeforeClosingFence ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaBeforeClosingFence = value;
		}
	}

	public int ClosingFencedCharCount { get; set; }

	public FencedCodeBlock(BlockParser parser)
		: base(parser)
	{
		base.IsBreakable = false;
	}
}
