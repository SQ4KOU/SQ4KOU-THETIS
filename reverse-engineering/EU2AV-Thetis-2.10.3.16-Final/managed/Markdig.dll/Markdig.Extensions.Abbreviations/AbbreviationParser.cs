using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Abbreviations;

public class AbbreviationParser : BlockParser
{
	public AbbreviationParser()
	{
		base.OpeningCharacters = new char[1] { '*' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		StringSlice lines = processor.Line;
		int start = lines.Start;
		if (lines.NextChar() != '[')
		{
			return BlockState.None;
		}
		if (!LinkHelper.TryParseLabel(ref lines, out string label, out SourceSpan labelSpan))
		{
			return BlockState.None;
		}
		if (lines.CurrentChar != ':')
		{
			return BlockState.None;
		}
		lines.SkipChar();
		lines.Trim();
		Abbreviation abbreviation = new Abbreviation(this)
		{
			Label = label,
			Text = lines,
			Span = new SourceSpan(start, lines.End),
			Line = processor.LineIndex,
			Column = processor.Column,
			LabelSpan = labelSpan
		};
		if (!processor.Document.HasAbbreviations())
		{
			processor.Document.ProcessInlinesEnd += DocumentOnProcessInlinesEnd;
		}
		processor.Document.AddAbbreviation(abbreviation.Label, abbreviation);
		return BlockState.BreakDiscard;
	}

	private void DocumentOnProcessInlinesEnd(InlineProcessor inlineProcessor, Inline? inline)
	{
		Dictionary<string, Abbreviation> abbreviations = inlineProcessor.Document.GetAbbreviations();
		if (abbreviations == null)
		{
			return;
		}
		CompactPrefixTree<Abbreviation> prefixTree = new CompactPrefixTree<Abbreviation>(abbreviations);
		Stack<ContainerInline> stack = new Stack<ContainerInline>();
		foreach (LeafBlock item in inlineProcessor.Document.Descendants<LeafBlock>())
		{
			if (item.Inline != null)
			{
				SubstituteInlineTree(item.Inline, prefixTree, stack);
			}
		}
	}

	private static void SubstituteInlineTree(ContainerInline root, CompactPrefixTree<Abbreviation> prefixTree, Stack<ContainerInline> stack)
	{
		stack.Push(root);
		while (stack.Count > 0)
		{
			Inline inline = stack.Pop().FirstChild;
			while (inline != null)
			{
				Inline nextSibling = inline.NextSibling;
				if (inline is LiteralInline literal)
				{
					SubstituteInLiteral(literal, prefixTree);
				}
				else if (inline is ContainerInline item)
				{
					stack.Push(item);
				}
				inline = nextSibling;
			}
		}
	}

	private static void SubstituteInLiteral(LiteralInline literal, CompactPrefixTree<Abbreviation> prefixTree)
	{
		StringSlice stringSlice = literal.Content;
		string text = stringSlice.Text;
		if (literal.Parent == null)
		{
			return;
		}
		int end = literal.Span.End;
		LiteralInline literalInline = literal;
		for (int i = stringSlice.Start; i <= stringSlice.End; i++)
		{
			if (i != stringSlice.Start)
			{
				i--;
				while (true)
				{
					if (i <= stringSlice.End)
					{
						if (text[i].IsWhitespace())
						{
							break;
						}
						i++;
						continue;
					}
					return;
				}
				i++;
			}
			if (!prefixTree.TryMatchLongest(text.AsSpan(i, stringSlice.End - i + 1), out KeyValuePair<string, Abbreviation> match))
			{
				continue;
			}
			string key = match.Key;
			if (IsValidAbbreviationEnding(key, stringSlice, i))
			{
				int num = i + key.Length;
				int num2 = i - literal.Content.Start;
				int num3 = literal.Span.Start + num2;
				AbbreviationInline abbreviationInline = new AbbreviationInline(match.Value)
				{
					Span = new SourceSpan(num3, num3 + key.Length - 1),
					Line = literal.Line,
					Column = literal.Column + num2
				};
				literalInline.Content.End = i - 1;
				literalInline.Span.End = num3 - 1;
				literalInline.InsertAfter(abbreviationInline);
				if (literalInline.Content.End < literalInline.Content.Start)
				{
					literalInline.Remove();
				}
				if (num > stringSlice.End)
				{
					break;
				}
				StringSlice stringSlice2 = stringSlice;
				stringSlice2.Start = num;
				LiteralInline literalInline2 = new LiteralInline
				{
					Content = stringSlice2,
					Span = new SourceSpan(abbreviationInline.Span.End + 1, end),
					Line = literal.Line,
					Column = literal.Column + (num - literal.Content.Start)
				};
				abbreviationInline.InsertAfter(literalInline2);
				literalInline = literalInline2;
				stringSlice = stringSlice2;
				i = num - 1;
			}
		}
	}

	private static bool IsValidAbbreviationEnding(string match, StringSlice content, int matchIndex)
	{
		StringSlice stringSlice = content;
		stringSlice.End = content.End + 1;
		for (int i = matchIndex + match.Length; i <= stringSlice.End; i++)
		{
			char c = stringSlice.PeekCharAbsolute(i);
			if (c.IsWhitespace())
			{
				break;
			}
			if (!c.IsAsciiPunctuationOrZero())
			{
				return false;
			}
		}
		return true;
	}
}
