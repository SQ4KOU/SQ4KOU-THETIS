using System;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Parsers;

public abstract class FencedBlockParserBase : BlockParser, IAttributesParseable
{
	public delegate bool InfoParserDelegate(BlockProcessor state, ref StringSlice line, IFencedBlock fenced, char openingCharacter);

	public InfoParserDelegate? InfoParser { get; set; }

	public TryParseAttributesDelegate? TryParseAttributes { get; set; }
}
public abstract class FencedBlockParserBase<T> : FencedBlockParserBase where T : Block, IFencedBlock
{
	private enum ParseState
	{
		AfterFence,
		Info,
		AfterInfo,
		Args,
		AfterArgs
	}

	private static readonly TransformedStringCache s_infoStringCache = new TransformedStringCache((string infoString) => HtmlHelper.Unescape(infoString));

	private static readonly TransformedStringCache s_argumentsStringCache = new TransformedStringCache((string argumentsString) => HtmlHelper.Unescape(argumentsString));

	private TransformedStringCache? _infoPrefixCache;

	private string? _infoPrefix;

	public string? InfoPrefix
	{
		get
		{
			return _infoPrefix;
		}
		set
		{
			if (_infoPrefix != value)
			{
				_infoPrefixCache = new TransformedStringCache((string infoString) => value + infoString);
				_infoPrefix = value;
			}
		}
	}

	public int MinimumMatchCount { get; set; }

	public int MaximumMatchCount { get; set; }

	protected FencedBlockParserBase()
	{
		base.InfoParser = DefaultInfoParser;
		MinimumMatchCount = 3;
		MaximumMatchCount = int.MaxValue;
	}

	public static bool RoundtripInfoParser(BlockProcessor blockProcessor, ref StringSlice line, IFencedBlock fenced, char openingCharacter)
	{
		int start = line.Start;
		int end = start - 1;
		StringSlice triviaAfterFencedChar = new StringSlice(line.Text, start, end);
		StringSlice unescapedInfo = new StringSlice(line.Text, start, end);
		StringSlice triviaAfterInfo = new StringSlice(line.Text, start, end);
		StringSlice unescapedArguments = new StringSlice(line.Text, start, end);
		StringSlice triviaAfterArguments = new StringSlice(line.Text, start, end);
		ParseState parseState = ParseState.AfterFence;
		for (int i = line.Start; i <= line.End; i++)
		{
			char c = line.Text[i];
			if (c == '`' && openingCharacter == '`')
			{
				return false;
			}
			switch (parseState)
			{
			case ParseState.AfterFence:
				if (c.IsSpaceOrTab())
				{
					triviaAfterFencedChar.End++;
					continue;
				}
				parseState = ParseState.Info;
				unescapedInfo.Start = i;
				unescapedInfo.End = i;
				triviaAfterFencedChar.End = i - 1;
				continue;
			case ParseState.Info:
				if (c.IsSpaceOrTab())
				{
					parseState = ParseState.AfterInfo;
					triviaAfterInfo.Start = i;
					triviaAfterInfo.End = i;
				}
				else
				{
					unescapedInfo.End++;
				}
				continue;
			case ParseState.AfterInfo:
				if (c.IsSpaceOrTab())
				{
					triviaAfterInfo.End++;
					continue;
				}
				unescapedArguments.Start = i;
				unescapedArguments.End = i;
				parseState = ParseState.Args;
				continue;
			case ParseState.Args:
				break;
			case ParseState.AfterArgs:
				return false;
			default:
				continue;
			}
			int num = line.End;
			while (num > start)
			{
				c = line[num];
				if (c.IsSpaceOrTab())
				{
					triviaAfterArguments.Start = i;
					num--;
					continue;
				}
				unescapedArguments.End = num;
				triviaAfterArguments.Start = num + 1;
				triviaAfterArguments.End = line.End;
				break;
			}
			break;
		}
		fenced.TriviaAfterFencedChar = triviaAfterFencedChar;
		fenced.Info = s_infoStringCache.Get(unescapedInfo.AsSpan());
		fenced.UnescapedInfo = unescapedInfo;
		fenced.TriviaAfterInfo = triviaAfterInfo;
		fenced.Arguments = HtmlHelper.Unescape(unescapedArguments.ToString());
		fenced.UnescapedArguments = unescapedArguments;
		fenced.TriviaAfterArguments = triviaAfterArguments;
		fenced.InfoNewLine = line.NewLine;
		return true;
	}

	public static bool DefaultInfoParser(BlockProcessor state, ref StringSlice line, IFencedBlock fenced, char openingCharacter)
	{
		int num = -1;
		ReadOnlySpan<char> span = line.AsSpan();
		if (!span.IsEmpty)
		{
			if (openingCharacter == '`')
			{
				num = span.IndexOfAny(' ', '\t', '`');
				if (num >= 0 && span.Slice(num).Contains('`'))
				{
					return false;
				}
			}
			else
			{
				num = span.IndexOfAny(' ', '\t');
			}
		}
		StringSlice stringSlice;
		if (num >= 0)
		{
			num += line.Start;
			stringSlice = new StringSlice(line.Text, line.Start, num - 1);
			StringSlice stringSlice2 = new StringSlice(line.Text, num, line.End);
			stringSlice2.Trim();
			fenced.Arguments = s_argumentsStringCache.Get(stringSlice2.AsSpan());
		}
		else
		{
			stringSlice = line;
			fenced.Arguments = string.Empty;
		}
		stringSlice.Trim();
		fenced.Info = s_infoStringCache.Get(stringSlice.AsSpan());
		return true;
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		StringSlice slice = processor.Line;
		char currentChar = slice.CurrentChar;
		int num = slice.CountAndSkipChar(currentChar);
		if (num < MinimumMatchCount || num > MaximumMatchCount)
		{
			return BlockState.None;
		}
		if (!processor.TrackTrivia)
		{
			slice.TrimStart();
		}
		T val = CreateFencedBlock(processor);
		val.Column = processor.Column;
		val.FencedChar = currentChar;
		val.OpeningFencedCharCount = num;
		val.Span.Start = processor.Start;
		val.Span.End = slice.Start;
		base.TryParseAttributes?.Invoke(processor, ref slice, val);
		if (base.InfoParser != null && !base.InfoParser(processor, ref slice, val, currentChar))
		{
			return BlockState.None;
		}
		string info = val.Info;
		if (!string.IsNullOrEmpty(info))
		{
			string name = _infoPrefixCache?.Get(info) ?? info;
			val.GetAttributes().AddClass(name);
		}
		processor.NewBlocks.Push(val);
		return BlockState.ContinueDiscard;
	}

	protected abstract T CreateFencedBlock(BlockProcessor processor);

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		IFencedBlock fencedBlock = (IFencedBlock)block;
		StringSlice line = processor.Line;
		int start = processor.Start;
		int num = line.CountAndSkipChar(fencedBlock.FencedChar);
		char currentChar = line.CurrentChar;
		int start2 = line.Start;
		if (fencedBlock.OpeningFencedCharCount <= num && !processor.IsCodeIndent && currentChar.IsWhiteSpaceOrZero() && line.TrimEnd())
		{
			block.UpdateSpanEnd(start2 - 1);
			fencedBlock.ClosingFencedCharCount = num;
			if (processor.TrackTrivia)
			{
				fencedBlock.NewLine = line.NewLine;
				fencedBlock.TriviaBeforeClosingFence = processor.UseTrivia(start - 1);
				fencedBlock.TriviaAfter = new StringSlice(line.Text, processor.Start + num, processor.Line.End);
			}
			return BlockState.BreakDiscard;
		}
		processor.GoToColumn(processor.ColumnBeforeIndent);
		return BlockState.Continue;
	}
}
