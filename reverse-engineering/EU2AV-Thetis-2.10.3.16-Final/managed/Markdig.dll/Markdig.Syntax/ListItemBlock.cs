using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax;

public class ListItemBlock : ContainerBlock
{
	private sealed class TriviaProperties
	{
		public StringSlice SourceBullet;
	}

	private TriviaProperties? _trivia => TryGetDerivedTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetDerivedTrivia<TriviaProperties>();

	public int ColumnWidth { get; set; }

	public int Order { get; set; }

	public StringSlice SourceBullet
	{
		get
		{
			return _trivia?.SourceBullet ?? StringSlice.Empty;
		}
		set
		{
			Trivia.SourceBullet = value;
		}
	}

	public ListItemBlock(BlockParser parser)
		: base(parser)
	{
	}
}
