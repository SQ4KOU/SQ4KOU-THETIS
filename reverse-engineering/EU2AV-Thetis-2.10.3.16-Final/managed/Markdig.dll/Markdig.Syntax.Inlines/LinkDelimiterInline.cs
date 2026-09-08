using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax.Inlines;

public class LinkDelimiterInline : DelimiterInline
{
	private sealed class TriviaProperties
	{
		public StringSlice LabelWithTrivia;
	}

	public SourceSpan LabelSpan;

	private TriviaProperties? _trivia => GetTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetTrivia<TriviaProperties>();

	public bool IsImage { get; set; }

	public string? Label { get; set; }

	public StringSlice LabelWithTrivia
	{
		get
		{
			return _trivia?.LabelWithTrivia ?? StringSlice.Empty;
		}
		set
		{
			Trivia.LabelWithTrivia = value;
		}
	}

	public LinkDelimiterInline(InlineParser parser)
		: base(parser)
	{
	}

	public override string ToLiteral()
	{
		if (!IsImage)
		{
			return "[";
		}
		return "![";
	}
}
