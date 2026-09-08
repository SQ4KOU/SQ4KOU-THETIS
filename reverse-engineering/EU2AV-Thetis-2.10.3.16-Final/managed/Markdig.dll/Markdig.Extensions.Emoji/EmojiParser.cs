using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Extensions.Emoji;

public class EmojiParser : InlineParser
{
	private readonly EmojiMapping _emojiMapping;

	public EmojiParser(EmojiMapping emojiMapping)
	{
		_emojiMapping = emojiMapping;
		base.OpeningCharacters = _emojiMapping.OpeningCharacters;
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (char.IsLetterOrDigit(slice.PeekCharExtra(-1)))
		{
			return false;
		}
		if (!_emojiMapping.PrefixTree.TryMatchLongest(slice.Text.AsSpan(slice.Start, slice.Length), out KeyValuePair<string, string> match))
		{
			return false;
		}
		processor.Inline = new EmojiInline(match.Value)
		{
			Span = 
			{
				Start = processor.GetSourcePosition(slice.Start, out var lineIndex, out var column)
			},
			Line = lineIndex,
			Column = column,
			Match = match.Key
		};
		processor.Inline.Span.End = processor.Inline.Span.Start + match.Key.Length - 1;
		slice.Start += match.Key.Length;
		return true;
	}
}
