using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace Markdig.Syntax;

public class LinkReferenceDefinition : LeafBlock
{
	public delegate Inline CreateLinkInlineDelegate(InlineProcessor inlineState, LinkReferenceDefinition linkRef, Inline? child = null);

	private sealed class TriviaProperties
	{
		public StringSlice LabelWithTrivia;

		public StringSlice TriviaBeforeUrl;

		public StringSlice UnescapedUrl;

		public bool UrlHasPointyBrackets;

		public StringSlice TriviaBeforeTitle;

		public StringSlice UnescapedTitle;

		public char TitleEnclosingCharacter;
	}

	public SourceSpan LabelSpan;

	public SourceSpan UrlSpan;

	public SourceSpan TitleSpan;

	private TriviaProperties? _trivia => TryGetDerivedTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetDerivedTrivia<TriviaProperties>();

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

	public StringSlice TriviaBeforeTitle
	{
		get
		{
			return _trivia?.TriviaBeforeTitle ?? StringSlice.Empty;
		}
		set
		{
			Trivia.TriviaBeforeTitle = value;
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

	public CreateLinkInlineDelegate? CreateLinkInline { get; set; }

	public LinkReferenceDefinition()
		: base(null)
	{
		base.IsOpen = false;
	}

	public LinkReferenceDefinition(string? label, string? url, string? title)
		: this()
	{
		Label = label;
		Url = url;
		Title = title;
	}

	public static bool TryParse<T>(ref T text, [NotNullWhen(true)] out LinkReferenceDefinition? block) where T : ICharIterator
	{
		block = null;
		int start = text.Start;
		if (!LinkHelper.TryParseLinkReferenceDefinition(ref text, out string label, out string url, out string title, out SourceSpan labelSpan, out SourceSpan urlSpan, out SourceSpan titleSpan))
		{
			return false;
		}
		block = new LinkReferenceDefinition(label, url, title)
		{
			LabelSpan = labelSpan,
			UrlSpan = urlSpan,
			TitleSpan = titleSpan,
			Span = new SourceSpan(start, (titleSpan.End > 0) ? titleSpan.End : urlSpan.End)
		};
		return true;
	}

	public static bool TryParseTrivia<T>(ref T text, [NotNullWhen(true)] out LinkReferenceDefinition? block, out SourceSpan triviaBeforeLabel, out SourceSpan labelWithTrivia, out SourceSpan triviaBeforeUrl, out SourceSpan unescapedUrl, out SourceSpan triviaBeforeTitle, out SourceSpan unescapedTitle, out SourceSpan triviaAfterTitle) where T : ICharIterator
	{
		block = null;
		int start = text.Start;
		if (!LinkHelper.TryParseLinkReferenceDefinitionTrivia(ref text, out triviaBeforeLabel, out string label, out labelWithTrivia, out triviaBeforeUrl, out string url, out unescapedUrl, out bool urlHasPointyBrackets, out triviaBeforeTitle, out string title, out unescapedTitle, out char titleEnclosingCharacter, out NewLine newLine, out triviaAfterTitle, out SourceSpan labelSpan, out SourceSpan urlSpan, out SourceSpan titleSpan))
		{
			return false;
		}
		block = new LinkReferenceDefinition(label, url, title)
		{
			UrlHasPointyBrackets = urlHasPointyBrackets,
			TitleEnclosingCharacter = titleEnclosingCharacter,
			LabelSpan = labelSpan,
			UrlSpan = urlSpan,
			TitleSpan = titleSpan,
			Span = new SourceSpan(start, (titleSpan.End > 0) ? titleSpan.End : urlSpan.End),
			NewLine = newLine
		};
		return true;
	}
}
