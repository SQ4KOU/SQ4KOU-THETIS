using System;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class AutolinkInlineParser : InlineParser
{
	public readonly AutolinkOptions Options;

	public AutolinkInlineParser()
		: this(new AutolinkOptions())
	{
	}

	public AutolinkInlineParser(AutolinkOptions options)
	{
		Options = options ?? throw new ArgumentNullException("options");
		base.OpeningCharacters = new char[1] { '<' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		StringSlice stringSlice = slice;
		int lineIndex;
		int column;
		if (LinkHelper.TryParseAutolink(ref slice, out string link, out bool isEmail))
		{
			processor.Inline = new AutolinkInline(link)
			{
				IsEmail = isEmail,
				Span = new SourceSpan(processor.GetSourcePosition(stringSlice.Start, out lineIndex, out column), processor.GetSourcePosition(slice.Start - 1)),
				Line = lineIndex,
				Column = column
			};
			if (Options.OpenInNewWindow)
			{
				processor.Inline.GetAttributes().AddPropertyIfNotExist("target", "_blank");
			}
		}
		else
		{
			if (!Options.EnableHtmlParsing)
			{
				return false;
			}
			slice = stringSlice;
			if (!HtmlHelper.TryParseHtmlTag(ref slice, out string htmlTag))
			{
				return false;
			}
			processor.Inline = new HtmlInline(htmlTag)
			{
				Span = new SourceSpan(processor.GetSourcePosition(stringSlice.Start, out lineIndex, out column), processor.GetSourcePosition(slice.Start - 1)),
				Line = lineIndex,
				Column = column
			};
			if (Options.OpenInNewWindow)
			{
				processor.Inline.GetAttributes().AddPropertyIfNotExist("target", "_blank");
			}
		}
		return true;
	}
}
