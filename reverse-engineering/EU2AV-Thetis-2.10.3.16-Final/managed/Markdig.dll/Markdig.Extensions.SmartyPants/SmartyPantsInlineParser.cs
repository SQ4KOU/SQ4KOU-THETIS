using System.Collections.Generic;
using System.Text;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.SmartyPants;

public class SmartyPantsInlineParser : InlineParser, IPostInlineProcessor
{
	private readonly struct Opener(int type, int index)
	{
		public readonly int Type = type;

		public readonly int Index = index;
	}

	private sealed class ListSmartyPants : List<SmartyPant>
	{
		public bool HasDash { get; set; }
	}

	public SmartyPantsInlineParser()
	{
		base.OpeningCharacters = new char[6] { '\'', '"', '<', '>', '.', '-' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		Rune pc = slice.PeekRuneExtra(-1);
		char currentChar = slice.CurrentChar;
		int start = slice.Start;
		SmartyPantType smartyPantType = (SmartyPantType)0;
		switch (currentChar)
		{
		case '\'':
			smartyPantType = SmartyPantType.Quote;
			if (slice.PeekChar() == '\'')
			{
				slice.SkipChar();
				smartyPantType = SmartyPantType.DoubleQuote;
			}
			break;
		case '"':
			smartyPantType = SmartyPantType.DoubleQuote;
			break;
		case '<':
			if (slice.NextChar() == '<')
			{
				smartyPantType = SmartyPantType.LeftAngleQuote;
			}
			break;
		case '>':
			if (slice.NextChar() == '>')
			{
				smartyPantType = SmartyPantType.RightAngleQuote;
			}
			break;
		case '.':
			if (slice.NextChar() == '.' && slice.NextChar() == '.')
			{
				smartyPantType = SmartyPantType.Ellipsis;
			}
			break;
		case '-':
			if (slice.NextChar() == '-')
			{
				GetOrCreateState(processor).HasDash = true;
				return false;
			}
			break;
		}
		if (smartyPantType == (SmartyPantType)0)
		{
			return false;
		}
		Rune c = slice.NextRune();
		CharHelper.CheckOpenCloseDelimiter(pc, c, enableWithinWord: false, out var canOpen, out var canClose);
		bool flag = false;
		switch (smartyPantType)
		{
		case SmartyPantType.Quote:
			flag = true;
			if (canOpen && !canClose)
			{
				smartyPantType = SmartyPantType.LeftQuote;
				break;
			}
			if (!canOpen & canClose)
			{
				smartyPantType = SmartyPantType.RightQuote;
				break;
			}
			return false;
		case SmartyPantType.DoubleQuote:
			flag = true;
			if (canOpen && !canClose)
			{
				smartyPantType = SmartyPantType.LeftDoubleQuote;
				break;
			}
			if (!canOpen & canClose)
			{
				smartyPantType = SmartyPantType.RightDoubleQuote;
				break;
			}
			return false;
		case SmartyPantType.LeftAngleQuote:
			flag = true;
			if (!canOpen | canClose)
			{
				return false;
			}
			break;
		case SmartyPantType.RightAngleQuote:
			flag = true;
			if (canOpen || !canClose)
			{
				return false;
			}
			break;
		case SmartyPantType.Ellipsis:
			if (canOpen || !canClose)
			{
				return false;
			}
			break;
		}
		SmartyPant smartyPant = new SmartyPant
		{
			Span = 
			{
				Start = processor.GetSourcePosition(start, out var lineIndex, out var column)
			},
			Line = lineIndex,
			Column = column,
			OpeningCharacter = currentChar,
			Type = smartyPantType
		};
		smartyPant.Span.End = smartyPant.Span.Start + slice.Start - start - 1;
		if (flag)
		{
			ListSmartyPants orCreateState = GetOrCreateState(processor);
			if (orCreateState.Count == 0)
			{
				processor.Block.ProcessInlinesEnd += BlockOnProcessInlinesEnd;
			}
			orCreateState.Add(smartyPant);
		}
		processor.Inline = smartyPant;
		return true;
	}

	private ListSmartyPants GetOrCreateState(InlineProcessor processor)
	{
		ListSmartyPants listSmartyPants = processor.ParserStates[base.Index] as ListSmartyPants;
		if (listSmartyPants == null)
		{
			listSmartyPants = (ListSmartyPants)(processor.ParserStates[base.Index] = new ListSmartyPants());
		}
		return listSmartyPants;
	}

	private void BlockOnProcessInlinesEnd(InlineProcessor processor, Inline? inline)
	{
		ListSmartyPants listSmartyPants = (ListSmartyPants)processor.ParserStates[base.Index];
		Stack<Opener> stack = new Stack<Opener>(4);
		for (int i = 0; i < listSmartyPants.Count; i++)
		{
			SmartyPant smartyPant = listSmartyPants[i];
			SmartyPantType type = smartyPant.Type;
			int num;
			bool flag;
			if ((uint)(type - 2) <= 1u)
			{
				num = 0;
				flag = type == SmartyPantType.LeftQuote;
			}
			else if ((uint)(type - 5) <= 1u)
			{
				num = 1;
				flag = type == SmartyPantType.LeftDoubleQuote;
			}
			else
			{
				if ((uint)(type - 7) > 1u)
				{
					smartyPant.ReplaceBy(smartyPant.AsLiteralInline());
					continue;
				}
				num = 2;
				flag = type == SmartyPantType.LeftAngleQuote;
			}
			if (flag)
			{
				stack.Push(new Opener(num, i));
				continue;
			}
			bool flag2 = false;
			while (stack.Count > 0)
			{
				Opener opener = stack.Pop();
				SmartyPant smartyPant2 = listSmartyPants[opener.Index];
				if (opener.Type == num)
				{
					flag2 = true;
					break;
				}
				smartyPant2.ReplaceBy(smartyPant2.AsLiteralInline());
			}
			if (!flag2)
			{
				smartyPant.ReplaceBy(smartyPant.AsLiteralInline());
			}
		}
		foreach (Opener item in stack)
		{
			SmartyPant smartyPant3 = listSmartyPants[item.Index];
			smartyPant3.ReplaceBy(smartyPant3.AsLiteralInline());
		}
		listSmartyPants.Clear();
	}

	bool IPostInlineProcessor.PostProcess(InlineProcessor state, Inline? root, Inline? lastChild, int postInlineProcessorIndex, bool isFinalProcessing)
	{
		if (!(state.ParserStates[base.Index] is ListSmartyPants { HasDash: not false }))
		{
			return true;
		}
		Inline inline = root;
		Stack<Inline> stack = new Stack<Inline>();
		while (true)
		{
			if (inline != null)
			{
				Inline inline2 = inline.NextSibling;
				if (inline is LiteralInline literalInline)
				{
					int offset = 0;
					int num = literalInline.Content.IndexOf("--", offset);
					if (num >= 0)
					{
						SmartyPantType smartyPantType = SmartyPantType.Dash2;
						if (literalInline.Content.PeekCharAbsolute(num + 2) == '-')
						{
							smartyPantType = SmartyPantType.Dash3;
						}
						StringSlice content = literalInline.Content;
						SourceSpan span = literalInline.Span;
						literalInline.Span.End -= literalInline.Content.End - num + 1;
						literalInline.Content.End = num - 1;
						content.Start = num + ((smartyPantType == SmartyPantType.Dash2) ? 2 : 3);
						SmartyPant smartyPant = new SmartyPant
						{
							Span = new SourceSpan(literalInline.Content.End + 1, content.Start - 1),
							Line = literalInline.Line,
							Column = literalInline.Column,
							OpeningCharacter = '-',
							Type = smartyPantType
						};
						literalInline.InsertAfter(smartyPant);
						LiteralInline literalInline2 = new LiteralInline
						{
							Span = new SourceSpan(smartyPant.Span.End + 1, span.End),
							Line = literalInline.Line,
							Column = literalInline.Column,
							Content = content
						};
						smartyPant.InsertAfter(literalInline2);
						inline2 = literalInline2;
					}
				}
				else if (inline is ContainerInline containerInline)
				{
					stack.Push(containerInline.FirstChild);
				}
				inline = inline2;
			}
			else
			{
				if (stack.Count <= 0)
				{
					break;
				}
				inline = stack.Pop();
			}
		}
		return true;
	}
}
