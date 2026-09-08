using System;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class CodeInlineParser : InlineParser
{
	public CodeInlineParser()
	{
		base.OpeningCharacters = new char[1] { '`' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char currentChar = slice.CurrentChar;
		if (slice.PeekCharExtra(-1) == currentChar)
		{
			return false;
		}
		int start = slice.Start;
		int num = slice.CountAndSkipChar(currentChar);
		ReadOnlySpan<char> span = slice.AsSpan();
		bool flag = false;
		while (true)
		{
			int i = span.IndexOfAny('\r', '\n', currentChar);
			if ((uint)i >= (uint)span.Length)
			{
				return false;
			}
			int num2 = 0;
			for (; (uint)i < (uint)span.Length && span[i] == currentChar; i++)
			{
				num2++;
			}
			span = span.Slice(i);
			if (num == num2)
			{
				break;
			}
			if (num2 == 0)
			{
				if (span.TrimStart(new ReadOnlySpan<char>(new char[2] { '\r', '\n' })).StartsWith('|') && processor.Inline != null && processor.Inline.ContainsParentOrSiblingOfType<PipeTableDelimiterInline>())
				{
					slice.Start = start;
					return false;
				}
				flag = true;
				span = span.Slice(1);
			}
		}
		ReadOnlySpan<char> readOnlySpan = slice.AsSpan().Slice(0, slice.Length - span.Length - num);
		LazySubstring content = (flag ? new LazySubstring(ReplaceNewLines(readOnlySpan)) : new LazySubstring(slice.Text, slice.Start, readOnlySpan.Length));
		bool flag2 = readOnlySpan.Length > 2;
		if (flag2)
		{
			char c = readOnlySpan[0];
			bool flag3 = ((c == '\n' || c == ' ') ? true : false);
			flag2 = flag3;
		}
		bool flag4 = flag2;
		if (flag4)
		{
			char c = readOnlySpan[readOnlySpan.Length - 1];
			bool flag3 = ((c == '\n' || c == ' ') ? true : false);
			flag4 = flag3;
		}
		if (flag4 && readOnlySpan.ContainsAnyExcept(' ', '\r', '\n'))
		{
			content.Offset++;
			content.Length -= 2;
		}
		int start2 = slice.Start;
		slice.Start = start2 + readOnlySpan.Length + num;
		start2 -= num;
		CodeInline codeInline = new CodeInline(content)
		{
			Delimiter = slice.Text[start2],
			Span = new SourceSpan(processor.GetSourcePosition(start2, out var lineIndex, out var column), processor.GetSourcePosition(slice.Start - 1)),
			Line = lineIndex,
			Column = column,
			DelimiterCount = num
		};
		if (processor.TrackTrivia)
		{
			codeInline.ContentWithTrivia = new StringSlice(slice.Text, start2 + num, slice.Start - num - 1);
		}
		processor.Inline = codeInline;
		return true;
	}

	private static string ReplaceNewLines(ReadOnlySpan<char> content)
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		while (true)
		{
			int num = content.IndexOfAny('\r', '\n');
			if ((uint)num >= (uint)content.Length)
			{
				break;
			}
			valueStringBuilder.Append(content.Slice(0, num));
			if (content[num] == '\n')
			{
				valueStringBuilder.Append(' ');
			}
			content = content.Slice(num + 1);
		}
		valueStringBuilder.Append(content);
		return valueStringBuilder.ToString();
	}
}
