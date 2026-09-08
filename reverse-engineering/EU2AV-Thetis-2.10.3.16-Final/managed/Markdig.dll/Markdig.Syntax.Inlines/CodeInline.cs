using System;
using System.Diagnostics;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("`{Content}`")]
public class CodeInline : LeafInline
{
	private sealed class TriviaProperties
	{
		public StringSlice ContentWithTrivia;
	}

	private LazySubstring _content;

	private TriviaProperties? _trivia => GetTrivia<TriviaProperties>();

	private TriviaProperties Trivia => GetOrSetTrivia<TriviaProperties>();

	public char Delimiter { get; set; }

	public int DelimiterCount { get; set; }

	public string Content
	{
		get
		{
			return _content.ToString();
		}
		set
		{
			_content = new LazySubstring(value ?? string.Empty);
		}
	}

	public ReadOnlySpan<char> ContentSpan => _content.AsSpan();

	public StringSlice ContentWithTrivia
	{
		get
		{
			return _trivia?.ContentWithTrivia ?? StringSlice.Empty;
		}
		set
		{
			Trivia.ContentWithTrivia = value;
		}
	}

	public CodeInline(string content)
		: this(new LazySubstring(content))
	{
	}

	internal CodeInline(LazySubstring content)
	{
		_content = content;
	}
}
