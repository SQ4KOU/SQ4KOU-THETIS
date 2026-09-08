using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public sealed class LiteralInlineParser : InlineParser
{
	public delegate void PostMatchDelegate(InlineProcessor processor, ref StringSlice slice);

	public PostMatchDelegate? PostMatch { get; set; }

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		string text = slice.Text;
		int num = processor.Parsers.IndexOfOpeningCharacter(text, slice.Start + 1, slice.End);
		int num2;
		if ((uint)num >= (uint)text.Length)
		{
			num = slice.End + 1;
			num2 = num - slice.Start;
		}
		else
		{
			num2 = num - slice.Start;
			if (!processor.TrackTrivia)
			{
				char c = text[num];
				if ((c == '\n' || c == '\r') ? true : false)
				{
					int num3 = num - 1;
					while (num2 > 0 && text[num3].IsSpace())
					{
						num2--;
						num3--;
					}
				}
			}
		}
		int num4 = slice.Start + num2 - 1;
		Inline inline = processor.Inline;
		if (inline != null && !inline.IsContainer && processor.Inline is LiteralInline literalInline && (object)literalInline.Content.Text == slice.Text && literalInline.Content.End + 1 == slice.Start)
		{
			literalInline.Content.End = num4;
			literalInline.Span.End = processor.GetSourcePosition(num4);
		}
		else
		{
			StringSlice stringSlice = ((num2 > 0) ? new StringSlice(slice.Text, slice.Start, num4) : StringSlice.Empty);
			if (!stringSlice.IsEmpty)
			{
				processor.Inline = new LiteralInline
				{
					Content = ((num2 > 0) ? stringSlice : StringSlice.Empty),
					Span = new SourceSpan(processor.GetSourcePosition(slice.Start, out var lineIndex, out var column), processor.GetSourcePosition(num4)),
					Line = lineIndex,
					Column = column
				};
			}
		}
		slice.Start = num;
		if (processor.Inline is LiteralInline)
		{
			PostMatch?.Invoke(processor, ref slice);
		}
		return true;
	}
}
