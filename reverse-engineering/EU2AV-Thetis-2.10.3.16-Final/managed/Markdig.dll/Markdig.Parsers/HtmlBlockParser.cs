using System;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class HtmlBlockParser : BlockParser
{
	private const string EndOfComment = "-->";

	private const string EndOfCDATA = "]]>";

	private const string EndOfProcessingInstruction = "?>";

	private static readonly CompactPrefixTree<int> HtmlTags = new CompactPrefixTree<int>(67, 96, 86)
	{
		{ "address", 0 },
		{ "article", 1 },
		{ "aside", 2 },
		{ "base", 3 },
		{ "basefont", 4 },
		{ "blockquote", 5 },
		{ "body", 6 },
		{ "caption", 7 },
		{ "center", 8 },
		{ "col", 9 },
		{ "colgroup", 10 },
		{ "dd", 11 },
		{ "details", 12 },
		{ "dialog", 13 },
		{ "dir", 14 },
		{ "div", 15 },
		{ "dl", 16 },
		{ "dt", 17 },
		{ "fieldset", 18 },
		{ "figcaption", 19 },
		{ "figure", 20 },
		{ "footer", 21 },
		{ "form", 22 },
		{ "frame", 23 },
		{ "frameset", 24 },
		{ "h1", 25 },
		{ "h2", 26 },
		{ "h3", 27 },
		{ "h4", 28 },
		{ "h5", 29 },
		{ "h6", 30 },
		{ "head", 31 },
		{ "header", 32 },
		{ "hr", 33 },
		{ "html", 34 },
		{ "iframe", 35 },
		{ "legend", 36 },
		{ "li", 37 },
		{ "link", 38 },
		{ "main", 39 },
		{ "menu", 40 },
		{ "menuitem", 41 },
		{ "nav", 42 },
		{ "noframes", 43 },
		{ "ol", 44 },
		{ "optgroup", 45 },
		{ "option", 46 },
		{ "p", 47 },
		{ "param", 48 },
		{ "pre", 49 },
		{ "script", 50 },
		{ "section", 51 },
		{ "source", 52 },
		{ "style", 53 },
		{ "summary", 54 },
		{ "table", 55 },
		{ "textarea", 56 },
		{ "tbody", 57 },
		{ "td", 58 },
		{ "tfoot", 59 },
		{ "th", 60 },
		{ "thead", 61 },
		{ "title", 62 },
		{ "tr", 63 },
		{ "track", 64 },
		{ "ul", 65 },
		{ "search", 66 }
	};

	public HtmlBlockParser()
	{
		base.OpeningCharacters = new char[1] { '<' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		BlockState blockState = MatchStart(processor);
		if (blockState == BlockState.Continue)
		{
			blockState = MatchEnd(processor, (HtmlBlock)processor.NewBlocks.Peek());
		}
		return blockState;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		HtmlBlock htmlBlock = (HtmlBlock)block;
		return MatchEnd(processor, htmlBlock);
	}

	private BlockState MatchStart(BlockProcessor state)
	{
		if (state.IsCodeIndent)
		{
			return BlockState.None;
		}
		StringSlice line = state.Line;
		int start = line.Start;
		line.SkipChar();
		BlockState blockState = TryParseTagType16(state, line, state.ColumnBeforeIndent, start);
		if (blockState == BlockState.None && !(state.CurrentBlock is ParagraphBlock))
		{
			blockState = TryParseTagType7(state, line, state.ColumnBeforeIndent, start);
		}
		return blockState;
	}

	private BlockState TryParseTagType7(BlockProcessor state, StringSlice line, int startColumn, int startPosition)
	{
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
		char currentChar = line.CurrentChar;
		BlockState result = BlockState.None;
		if ((currentChar == '/' && HtmlHelper.TryParseHtmlCloseTag(ref line, ref builder)) || HtmlHelper.TryParseHtmlTagOpenTag(ref line, ref builder))
		{
			bool flag = true;
			for (currentChar = line.CurrentChar; currentChar != 0; currentChar = line.NextChar())
			{
				if (!currentChar.IsWhitespace())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result = CreateHtmlBlock(state, HtmlBlockType.NonInterruptingBlock, startColumn, startPosition);
			}
		}
		builder.Dispose();
		return result;
	}

	private BlockState TryParseTagType16(BlockProcessor state, StringSlice line, int startColumn, int startPosition)
	{
		char c = line.CurrentChar;
		switch (c)
		{
		case '!':
			c = line.NextChar();
			if (c == '-' && line.PeekChar() == '-')
			{
				return CreateHtmlBlock(state, HtmlBlockType.Comment, startColumn, startPosition);
			}
			if (c.IsAlphaUpper())
			{
				return CreateHtmlBlock(state, HtmlBlockType.DocumentType, startColumn, startPosition);
			}
			if (c == '[' && line.Match("CDATA[", 1))
			{
				return CreateHtmlBlock(state, HtmlBlockType.CData, startColumn, startPosition);
			}
			return BlockState.None;
		case '?':
			return CreateHtmlBlock(state, HtmlBlockType.ProcessingInstruction, startColumn, startPosition);
		default:
		{
			bool flag = c == '/';
			if (flag)
			{
				c = line.NextChar();
			}
			Span<char> span = stackalloc char[10];
			int i;
			for (i = 0; i < span.Length; i++)
			{
				if (!c.IsAlphaNumeric())
				{
					break;
				}
				span[i] = char.ToLowerInvariant(c);
				c = line.NextChar();
			}
			if (c != '>' && (flag || c != '/' || line.PeekChar() != '>') && !c.IsWhiteSpaceOrZero())
			{
				return BlockState.None;
			}
			if (i == 0)
			{
				return BlockState.None;
			}
			if (!HtmlTags.TryMatchExact(span.Slice(0, i), out var match))
			{
				return BlockState.None;
			}
			int value = match.Value;
			if (value == 49 || value == 50 || value == 53 || value == 56)
			{
				if ((c == '/') | flag)
				{
					return BlockState.None;
				}
				return CreateHtmlBlock(state, HtmlBlockType.ScriptPreOrStyle, startColumn, startPosition);
			}
			return CreateHtmlBlock(state, HtmlBlockType.InterruptingBlock, startColumn, startPosition);
		}
		}
	}

	private BlockState MatchEnd(BlockProcessor state, HtmlBlock htmlBlock)
	{
		state.GoToColumn(state.ColumnBeforeIndent);
		StringSlice line = state.Line;
		BlockState blockState = BlockState.Continue;
		switch (htmlBlock.Type)
		{
		case HtmlBlockType.Comment:
		{
			int num = line.IndexOf("-->");
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "-->".Length);
				blockState = BlockState.Break;
			}
			break;
		}
		case HtmlBlockType.CData:
		{
			int num = line.IndexOf("]]>");
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "]]>".Length);
				blockState = BlockState.Break;
			}
			break;
		}
		case HtmlBlockType.ProcessingInstruction:
		{
			int num = line.IndexOf("?>");
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "?>".Length);
				blockState = BlockState.Break;
			}
			break;
		}
		case HtmlBlockType.DocumentType:
		{
			int num = line.IndexOf('>');
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + 1);
				blockState = BlockState.Break;
			}
			break;
		}
		case HtmlBlockType.ScriptPreOrStyle:
		{
			int num = line.IndexOf("</script>", 0, ignoreCase: true);
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "</script>".Length);
				blockState = BlockState.Break;
				break;
			}
			num = line.IndexOf("</pre>", 0, ignoreCase: true);
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "</pre>".Length);
				blockState = BlockState.Break;
				break;
			}
			num = line.IndexOf("</style>", 0, ignoreCase: true);
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "</style>".Length);
				blockState = BlockState.Break;
				break;
			}
			num = line.IndexOf("</textarea>", 0, ignoreCase: true);
			if (num >= 0)
			{
				htmlBlock.UpdateSpanEnd(num + "</textarea>".Length);
				blockState = BlockState.Break;
			}
			break;
		}
		case HtmlBlockType.InterruptingBlock:
			if (state.IsBlankLine)
			{
				blockState = BlockState.BreakDiscard;
			}
			break;
		case HtmlBlockType.NonInterruptingBlock:
			if (state.IsBlankLine)
			{
				blockState = BlockState.BreakDiscard;
			}
			break;
		}
		if (blockState != BlockState.BreakDiscard)
		{
			htmlBlock.Span.End = line.End;
			htmlBlock.NewLine = state.Line.NewLine;
		}
		return blockState;
	}

	private BlockState CreateHtmlBlock(BlockProcessor state, HtmlBlockType type, int startColumn, int startPosition)
	{
		HtmlBlock htmlBlock = new HtmlBlock(this)
		{
			Column = startColumn,
			Type = type,
			Span = new SourceSpan(startPosition, startPosition + state.Line.End)
		};
		if (state.TrackTrivia)
		{
			htmlBlock.LinesBefore = state.TakeLinesBefore();
			htmlBlock.NewLine = state.Line.NewLine;
		}
		state.NewBlocks.Push(htmlBlock);
		return BlockState.Continue;
	}
}
