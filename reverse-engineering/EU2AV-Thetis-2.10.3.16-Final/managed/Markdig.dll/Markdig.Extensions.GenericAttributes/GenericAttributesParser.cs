using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.GenericAttributes;

public class GenericAttributesParser : InlineParser
{
	public GenericAttributesParser()
	{
		base.OpeningCharacters = new char[1] { '{' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		int start = slice.Start;
		if (TryParse(ref slice, out HtmlAttributes attributes))
		{
			Inline inline = processor.Inline;
			if (inline is LiteralInline)
			{
				do
				{
					inline = inline.Parent;
				}
				while (inline is DelimiterInline);
			}
			MarkdownObject markdownObject = ((inline == null || inline == processor.Root) ? ((MarkdownObject)processor.Block) : ((MarkdownObject)inline));
			if (markdownObject is ParagraphBlock paragraphBlock && paragraphBlock.Inline.FirstChild == null && processor.Inline == null && slice.IsEmptyOrWhitespace())
			{
				ContainerBlock parent = paragraphBlock.Parent;
				int num = parent.IndexOf(paragraphBlock);
				if (num + 1 < parent.Count)
				{
					markdownObject = parent[num + 1];
					paragraphBlock.RemoveAfterProcessInlines = true;
				}
			}
			HtmlAttributes attributes2 = markdownObject.GetAttributes();
			attributes.CopyTo(attributes2, mergeIdAndProperties: true, shared: false);
			attributes2.Span.Start = processor.GetSourcePosition(start, out var lineIndex, out var column);
			attributes2.Line = lineIndex;
			attributes2.Column = column;
			attributes2.Span.End = attributes2.Span.Start + slice.Start - start - 1;
			return true;
		}
		return false;
	}

	public static bool TryParse(ref StringSlice slice, [NotNullWhen(true)] out HtmlAttributes? attributes)
	{
		attributes = null;
		if (slice.PeekCharExtra(-1) == '{')
		{
			return false;
		}
		StringSlice stringSlice = slice;
		string id = null;
		List<string> list = null;
		List<KeyValuePair<string, string>> list2 = null;
		bool flag = false;
		char c = stringSlice.NextChar();
		while (true)
		{
			switch (c)
			{
			case '}':
				flag = true;
				stringSlice.SkipChar();
				if (stringSlice.CurrentChar == '\n')
				{
					stringSlice.SkipChar();
				}
				else if (stringSlice.CurrentChar == '\r' && stringSlice.PeekChar() == '\n')
				{
					stringSlice.Start += 2;
				}
				break;
			default:
			{
				bool flag2 = c == '.';
				if ((c == '#') | flag2)
				{
					c = stringSlice.NextChar();
					int start = stringSlice.Start;
					while (c != '}' && !c.IsWhiteSpaceOrZero())
					{
						c = stringSlice.NextChar();
					}
					int num = stringSlice.Start - 1;
					if (num == start)
					{
						break;
					}
					string text = slice.Text.Substring(start, num - start + 1);
					if (flag2)
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(text);
					}
					else
					{
						id = text;
					}
					continue;
				}
				if (!c.IsWhitespace())
				{
					if (!IsStartAttributeName(c))
					{
						break;
					}
					int start2 = stringSlice.Start;
					do
					{
						c = stringSlice.NextChar();
					}
					while (c.IsAlphaNumeric() || c == '_' || c == ':' || c == '.' || c == '-');
					string key = slice.Text.Substring(start2, stringSlice.Start - start2);
					bool num2 = c.IsSpaceOrTab();
					stringSlice.TrimStart();
					c = stringSlice.CurrentChar;
					if ((num2 && (c == '.' || c == '#' || IsStartAttributeName(c))) || c == '}')
					{
						if (list2 == null)
						{
							list2 = new List<KeyValuePair<string, string>>();
						}
						list2.Add(new KeyValuePair<string, string>(key, null));
						continue;
					}
					if (stringSlice.CurrentChar != '=')
					{
						break;
					}
					stringSlice.SkipChar();
					stringSlice.TrimStart();
					int num3 = -1;
					int num4 = -1;
					c = stringSlice.CurrentChar;
					if (c == '\'' || c == '"')
					{
						char c2 = c;
						num3 = stringSlice.Start + 1;
						do
						{
							c = stringSlice.NextChar();
							if (c == '\0')
							{
								return false;
							}
						}
						while (c != c2);
						num4 = stringSlice.Start - 1;
						c = stringSlice.NextChar();
					}
					else
					{
						num3 = stringSlice.Start;
						bool flag3 = false;
						while (true)
						{
							if (c == '\0')
							{
								return false;
							}
							if (c.IsWhitespace() || c == '}')
							{
								break;
							}
							c = stringSlice.NextChar();
							flag3 = true;
						}
						num4 = stringSlice.Start - 1;
						if (!flag3)
						{
							break;
						}
					}
					string value = slice.Text.Substring(num3, num4 - num3 + 1);
					if (list2 == null)
					{
						list2 = new List<KeyValuePair<string, string>>();
					}
					list2.Add(new KeyValuePair<string, string>(key, value));
					continue;
				}
				c = stringSlice.NextChar();
				continue;
			}
			case '\0':
				break;
			}
			break;
		}
		if (flag)
		{
			attributes = new HtmlAttributes
			{
				Id = id,
				Classes = list,
				Properties = list2
			};
			slice = stringSlice;
		}
		return flag;
	}

	private static bool IsStartAttributeName(char c)
	{
		if (!c.IsAlpha() && c != '_')
		{
			return c == ':';
		}
		return true;
	}
}
