using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Extensions.Alerts;

public class AlertInlineParser : InlineParser
{
	private static readonly TransformedStringCache s_alertTypeClassCache = new TransformedStringCache((string type) => "markdown-alert-" + type.ToLowerInvariant());

	public AlertInlineParser()
	{
		base.OpeningCharacters = new char[1] { '[' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (slice.PeekChar() != '!')
		{
			return false;
		}
		if (!(processor.Block is ParagraphBlock { Parent: QuoteBlock parent } paragraphBlock) || paragraphBlock.Inline?.FirstChild != null || parent is AlertBlock || !(parent.Parent is MarkdownDocument))
		{
			return false;
		}
		StringSlice stringSlice = slice;
		slice.SkipChar();
		char c = slice.NextChar();
		int start = slice.Start;
		int num = start;
		while (c.IsAlpha())
		{
			num = slice.Start;
			c = slice.NextChar();
		}
		if (c != ']' || start == num)
		{
			slice = stringSlice;
			return false;
		}
		StringSlice kind = new StringSlice(slice.Text, start, num);
		c = slice.NextChar();
		start = slice.Start;
		while (true)
		{
			if (c == '\0' || c == '\n' || c == '\r')
			{
				num = slice.Start;
				switch (c)
				{
				case '\r':
					c = slice.NextChar();
					if (c == '\0' || c == '\n')
					{
						num = slice.Start;
						if (c == '\n')
						{
							slice.SkipChar();
						}
					}
					break;
				case '\n':
					slice.SkipChar();
					break;
				}
				AlertBlock alertBlock = new AlertBlock(kind)
				{
					Span = parent.Span,
					TriviaSpaceAfterKind = new StringSlice(slice.Text, start, num),
					Line = parent.Line,
					Column = parent.Column
				};
				HtmlAttributes attributes = alertBlock.GetAttributes();
				attributes.AddClass("markdown-alert");
				attributes.AddClass(s_alertTypeClassCache.Get(kind.AsSpan()));
				parent.ReplaceBy(alertBlock);
				processor.ReplaceParentContainer(parent, alertBlock);
				return true;
			}
			if (!c.IsSpaceOrTab())
			{
				break;
			}
			c = slice.NextChar();
		}
		slice = stringSlice;
		return false;
	}
}
