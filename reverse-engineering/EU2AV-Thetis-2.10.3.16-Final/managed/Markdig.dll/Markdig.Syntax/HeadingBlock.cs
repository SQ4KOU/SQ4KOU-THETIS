using System.Diagnostics;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

[DebuggerDisplay("{GetType().Name} Line: {Line}, {Lines} Level: {Level}")]
public class HeadingBlock : LeafBlock
{
	private sealed class TriviaProperties
	{
		public NewLine SetextNewline;

		public StringSlice TriviaAfterAtxHeaderChar;
	}

	private TriviaProperties? _trivia => TryGetDerivedTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetDerivedTrivia<TriviaProperties>();

	public char HeaderChar { get; set; }

	public int Level { get; set; }

	public bool IsSetext { get; set; }

	public int HeaderCharCount { get; set; }

	public NewLine SetextNewline
	{
		get
		{
			return _trivia?.SetextNewline ?? NewLine.None;
		}
		set
		{
			Trivia.SetextNewline = value;
		}
	}

	public StringSlice TriviaAfterAtxHeaderChar
	{
		get
		{
			return _trivia?.TriviaAfterAtxHeaderChar ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterAtxHeaderChar = value;
		}
	}

	public HeadingBlock(BlockParser parser)
		: base(parser)
	{
		base.ProcessInlines = true;
	}
}
