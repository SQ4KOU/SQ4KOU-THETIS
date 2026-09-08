using System.Diagnostics;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("Url: {Url} Title: {Title} Image: {IsImage}")]
public class LinkInline : ContainerInline
{
	public delegate string GetUrlDelegate();

	private sealed class TriviaProperties
	{
		public StringSlice LabelWithTrivia;

		public LocalLabel LocalLabel;

		public string? LinkRefDefLabel;

		public StringSlice LinkRefDefLabelWithTrivia;

		public StringSlice TriviaBeforeUrl;

		public bool UrlHasPointyBrackets;

		public StringSlice UnescapedUrl;

		public StringSlice TriviaAfterUrl;

		public char TitleEnclosingCharacter;

		public StringSlice UnescapedTitle;

		public StringSlice TriviaAfterTitle;
	}

	public SourceSpan LabelSpan;

	public SourceSpan UrlSpan;

	public SourceSpan TitleSpan;

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

	public LocalLabel LocalLabel
	{
		get
		{
			return _trivia?.LocalLabel ?? LocalLabel.None;
		}
		set
		{
			Trivia.LocalLabel = value;
		}
	}

	public LinkReferenceDefinition? Reference { get; set; }

	public string? LinkRefDefLabel
	{
		get
		{
			return _trivia?.LinkRefDefLabel;
		}
		set
		{
			Trivia.LinkRefDefLabel = value;
		}
	}

	public StringSlice LinkRefDefLabelWithTrivia
	{
		get
		{
			return _trivia?.LinkRefDefLabelWithTrivia ?? StringSlice.Empty;
		}
		set
		{
			Trivia.LinkRefDefLabelWithTrivia = value;
		}
	}

	public StringSlice TriviaBeforeUrl
	{
		get
		{
			return _trivia?.TriviaBeforeUrl ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaBeforeUrl = value;
		}
	}

	public bool UrlHasPointyBrackets
	{
		get
		{
			return _trivia?.UrlHasPointyBrackets ?? false;
		}
		set
		{
			Trivia.UrlHasPointyBrackets = value;
		}
	}

	public string? Url { get; set; }

	public StringSlice UnescapedUrl
	{
		get
		{
			return _trivia?.UnescapedUrl ?? StringSlice.Empty;
		}
		set
		{
			Trivia.UnescapedUrl = value;
		}
	}

	public StringSlice TriviaAfterUrl
	{
		get
		{
			return _trivia?.TriviaAfterUrl ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterUrl = value;
		}
	}

	public GetUrlDelegate? GetDynamicUrl { get; set; }

	public char TitleEnclosingCharacter
	{
		get
		{
			return _trivia?.TitleEnclosingCharacter ?? '\0';
		}
		set
		{
			Trivia.TitleEnclosingCharacter = value;
		}
	}

	public string? Title { get; set; }

	public StringSlice UnescapedTitle
	{
		get
		{
			return _trivia?.UnescapedTitle ?? StringSlice.Empty;
		}
		set
		{
			Trivia.UnescapedTitle = value;
		}
	}

	public StringSlice TriviaAfterTitle
	{
		get
		{
			return _trivia?.TriviaAfterTitle ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaAfterTitle = value;
		}
	}

	public bool IsShortcut { get; set; }

	public bool IsAutoLink { get; set; }

	public LinkInline()
	{
	}

	public LinkInline(string url, string title)
	{
		Url = url;
		Title = title;
	}
}
