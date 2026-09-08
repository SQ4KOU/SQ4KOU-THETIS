using System;
using System.Buffers;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.AutoLinks;

public class AutoLinkParser : InlineParser
{
	public readonly AutoLinkOptions Options;

	private readonly SearchValues<char> _validPreviousCharacters;

	public AutoLinkParser(AutoLinkOptions options)
	{
		Options = options ?? throw new ArgumentNullException("options");
		base.OpeningCharacters = new char[5] { 'h', 'f', 'm', 't', 'w' };
		_validPreviousCharacters = SearchValues.Create(options.ValidPreviousCharacters);
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char c = slice.PeekCharExtra(-1);
		if (!c.IsWhiteSpaceOrZero() && !_validPreviousCharacters.Contains(c))
		{
			return false;
		}
		ReadOnlySpan<char> span = slice.AsSpan();
		bool flag = span.Length >= 4;
		if (flag)
		{
			flag = span[0] switch
			{
				'h' => span.StartsWith("https://", StringComparison.Ordinal) || span.StartsWith("http://", StringComparison.Ordinal), 
				'w' => span.StartsWith("www.", StringComparison.Ordinal), 
				'f' => span.StartsWith("ftp://", StringComparison.Ordinal), 
				'm' => span.StartsWith("mailto:", StringComparison.Ordinal), 
				_ => span.StartsWith("tel:", StringComparison.Ordinal), 
			};
		}
		if (flag)
		{
			return MatchCore(processor, ref slice);
		}
		return false;
	}

	private bool MatchCore(InlineProcessor processor, ref StringSlice slice)
	{
		char currentChar = slice.CurrentChar;
		int start = slice.Start;
		Span<char> initialBuffer = stackalloc char[32];
		ValueStringBuilder pendingEmphasis = new ValueStringBuilder(initialBuffer);
		if (!IsAutoLinkValidInCurrentContext(processor, ref pendingEmphasis))
		{
			return false;
		}
		if (!LinkHelper.TryParseUrl(ref slice, out string link, out bool _, isAutoLink: true))
		{
			return false;
		}
		if (pendingEmphasis.Length > 0)
		{
			int num = link.Length - 1;
			while (num >= 0)
			{
				if (pendingEmphasis.AsSpan().Contains(link[num]))
				{
					slice.Start--;
					num--;
					continue;
				}
				if (num < link.Length - 1)
				{
					link = link.Substring(0, num + 1);
				}
				break;
			}
		}
		int prefixLength = 0;
		switch (currentChar)
		{
		case 'h':
			if (string.Equals(link, "http://", StringComparison.Ordinal) || string.Equals(link, "https://", StringComparison.Ordinal))
			{
				return false;
			}
			prefixLength = ((link[4] == 's') ? 8 : 7);
			break;
		case 'w':
			prefixLength = 4;
			break;
		case 'f':
			if (string.Equals(link, "ftp://", StringComparison.Ordinal))
			{
				return false;
			}
			prefixLength = 6;
			break;
		case 't':
			if (string.Equals(link, "tel", StringComparison.Ordinal))
			{
				return false;
			}
			break;
		case 'm':
		{
			int num2 = link.IndexOf('@');
			if (num2 == -1 || num2 == 7)
			{
				return false;
			}
			prefixLength = num2 + 1;
			break;
		}
		}
		if (currentChar != 't' && !LinkHelper.IsValidDomain(link, prefixLength, Options.AllowDomainWithoutPeriod))
		{
			return false;
		}
		LinkInline linkInline = new LinkInline
		{
			Span = 
			{
				Start = processor.GetSourcePosition(start, out var lineIndex, out var column)
			},
			Line = lineIndex,
			Column = column,
			Url = ((currentChar == 'w') ? ((Options.UseHttpsForWWWLinks ? "https://" : "http://") + link) : link),
			IsClosed = true,
			IsAutoLink = true
		};
		int num3 = currentChar switch
		{
			'm' => 7, 
			't' => 4, 
			_ => 0, 
		};
		linkInline.Span.End = linkInline.Span.Start + link.Length - 1;
		linkInline.UrlSpan = linkInline.Span;
		linkInline.AppendChild(new LiteralInline
		{
			Span = linkInline.Span,
			Line = lineIndex,
			Column = column,
			Content = new StringSlice(slice.Text, start + num3, start + link.Length - 1),
			IsClosed = true
		});
		processor.Inline = linkInline;
		if (Options.OpenInNewWindow)
		{
			linkInline.GetAttributes().AddPropertyIfNotExist("target", "_blank");
		}
		return true;
	}

	private static bool IsAutoLinkValidInCurrentContext(InlineProcessor processor, ref ValueStringBuilder pendingEmphasis)
	{
		Inline inline;
		for (inline = processor.Inline; inline != null; inline = inline.PreviousSibling ?? inline.Parent)
		{
			if (inline is HtmlInline htmlInline)
			{
				if (htmlInline.Tag.StartsWith("</a", StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				if (htmlInline.Tag.StartsWith("<a", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
		}
		inline = processor.Inline;
		int num = 0;
		while (inline != null)
		{
			if (inline is LinkDelimiterInline { IsActive: not false } linkDelimiterInline)
			{
				if (linkDelimiterInline.Type == DelimiterType.Open)
				{
					num++;
				}
				else if (linkDelimiterInline.Type == DelimiterType.Close)
				{
					num--;
				}
			}
			else if (inline is EmphasisDelimiterInline emphasisDelimiterInline && !pendingEmphasis.AsSpan().Contains(emphasisDelimiterInline.DelimiterChar))
			{
				pendingEmphasis.Append(emphasisDelimiterInline.DelimiterChar);
			}
			inline = inline.Parent;
		}
		return num <= 0;
	}
}
