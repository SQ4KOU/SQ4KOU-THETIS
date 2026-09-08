using System;
using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class HtmlEntityParser : InlineParser
{
	public HtmlEntityParser()
	{
		base.OpeningCharacters = new char[1] { '&' };
	}

	public static bool TryParse(ref StringSlice slice, [NotNullWhen(true)] out string? literal, out int match)
	{
		literal = null;
		match = HtmlHelper.ScanEntity(slice, out var numericEntity, out var namedEntityStart, out var namedEntityLength);
		if (match == 0)
		{
			return false;
		}
		if (namedEntityLength > 0)
		{
			literal = EntityHelper.DecodeEntity(slice.Text.AsSpan(namedEntityStart, namedEntityLength));
		}
		else if (numericEntity >= 0)
		{
			literal = EntityHelper.DecodeEntity(numericEntity);
		}
		return literal != null;
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (!TryParse(ref slice, out string literal, out int match))
		{
			return false;
		}
		int start = slice.Start;
		if (literal != null)
		{
			StringSlice original = slice;
			original.End = slice.Start + match - 1;
			processor.Inline = new HtmlEntityInline
			{
				Original = original,
				Transcoded = new StringSlice(literal),
				Span = new SourceSpan(processor.GetSourcePosition(start, out var lineIndex, out var column), processor.GetSourcePosition(original.End)),
				Line = lineIndex,
				Column = column
			};
			slice.Start += match;
			return true;
		}
		return false;
	}
}
