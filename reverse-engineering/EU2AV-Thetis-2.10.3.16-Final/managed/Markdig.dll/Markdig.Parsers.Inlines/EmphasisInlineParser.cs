using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class EmphasisInlineParser : InlineParser, IPostInlineProcessor
{
	public delegate EmphasisInline? TryCreateEmphasisInlineDelegate(char emphasisChar, int delimiterCount);

	public class DelimitersObjectCache : ObjectCache<List<EmphasisDelimiterInline>>
	{
		protected override List<EmphasisDelimiterInline> NewInstance()
		{
			return new List<EmphasisDelimiterInline>(4);
		}

		protected override void Reset(List<EmphasisDelimiterInline> instance)
		{
			instance.Clear();
		}
	}

	private CharacterMap<EmphasisDescriptor>? emphasisMap;

	private readonly DelimitersObjectCache inlinesCache = new DelimitersObjectCache();

	public readonly List<TryCreateEmphasisInlineDelegate> TryCreateEmphasisInlineList = new List<TryCreateEmphasisInlineDelegate>();

	public List<EmphasisDescriptor> EmphasisDescriptors { get; }

	public bool CjkFriendlyEmphasis { get; set; }

	public EmphasisInlineParser()
	{
		EmphasisDescriptors = new List<EmphasisDescriptor>(2)
		{
			new EmphasisDescriptor('*', 1, 2, enableWithinWord: true),
			new EmphasisDescriptor('_', 1, 2, enableWithinWord: false)
		};
	}

	public bool HasEmphasisChar(char c)
	{
		foreach (EmphasisDescriptor emphasisDescriptor in EmphasisDescriptors)
		{
			if (emphasisDescriptor.Character == c)
			{
				return true;
			}
		}
		return false;
	}

	public override void Initialize()
	{
		base.OpeningCharacters = new char[EmphasisDescriptors.Count];
		List<KeyValuePair<char, EmphasisDescriptor>> list = new List<KeyValuePair<char, EmphasisDescriptor>>();
		for (int i = 0; i < EmphasisDescriptors.Count; i++)
		{
			EmphasisDescriptor emphasisDescriptor = EmphasisDescriptors[i];
			if (Array.IndexOf(base.OpeningCharacters, emphasisDescriptor.Character) >= 0)
			{
				ThrowHelper.InvalidOperationException($"The character `{emphasisDescriptor.Character}` is already used by another emphasis descriptor");
			}
			base.OpeningCharacters[i] = emphasisDescriptor.Character;
			list.Add(new KeyValuePair<char, EmphasisDescriptor>(emphasisDescriptor.Character, emphasisDescriptor));
		}
		emphasisMap = new CharacterMap<EmphasisDescriptor>(list);
	}

	public bool PostProcess(InlineProcessor state, Inline? root, Inline? lastChild, int postInlineProcessorIndex, bool isFinalProcessing)
	{
		if (root == null || !root.IsContainerInline)
		{
			return true;
		}
		ContainerInline containerInline = Unsafe.As<ContainerInline>(root);
		List<EmphasisDelimiterInline> list = null;
		if (containerInline is EmphasisDelimiterInline item)
		{
			list = inlinesCache.Get();
			list.Add(item);
		}
		Inline inline = containerInline.FirstChild;
		while (inline != null && inline != lastChild)
		{
			if (inline.IsContainer && inline is DelimiterInline delimiterInline)
			{
				if (delimiterInline is EmphasisDelimiterInline item2)
				{
					if (list == null)
					{
						list = inlinesCache.Get();
					}
					list.Add(item2);
				}
				inline = delimiterInline.FirstChild ?? delimiterInline.NextSibling;
			}
			else
			{
				inline = inline.NextSibling;
			}
		}
		if (list != null)
		{
			ProcessEmphasis(state, list);
			inlinesCache.Release(list);
		}
		return true;
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char currentChar = slice.CurrentChar;
		EmphasisDescriptor emphasisDescriptor = emphasisMap[currentChar];
		Rune rune = (Rune)0;
		Rune twoPreviousRune = default(Rune);
		if (processor.Inline is HtmlEntityInline { Transcoded: { Length: >0 }, Transcoded: var transcoded2 } htmlEntityInline)
		{
			rune = transcoded2.RuneAt(htmlEntityInline.Transcoded.End);
			if (CjkFriendlyEmphasis)
			{
				twoPreviousRune = htmlEntityInline.Transcoded.RuneAt(htmlEntityInline.Transcoded.End - rune.Utf16SequenceLength);
			}
		}
		if (rune.Value == 0)
		{
			rune = slice.PeekRuneExtra(-1);
			if (CjkFriendlyEmphasis)
			{
				twoPreviousRune = slice.PeekRuneExtra(-1 - rune.Utf16SequenceLength);
			}
			if (rune == (Rune)currentChar && slice.PeekCharExtra(-2) != '\\')
			{
				return false;
			}
		}
		int start = slice.Start;
		int num = slice.CountAndSkipChar(currentChar);
		if (num < emphasisDescriptor.MinimumCount)
		{
			return false;
		}
		Rune result = slice.CurrentRune;
		if (HtmlEntityParser.TryParse(ref slice, out string literal, out int _))
		{
			Rune.DecodeFromUtf16(literal.AsSpan(), out result, out var _);
		}
		bool canOpen;
		bool canClose;
		if (CjkFriendlyEmphasis)
		{
			CharHelper.CheckOpenCloseDelimiterCjkFriendly(rune, result, twoPreviousRune, emphasisDescriptor.EnableWithinWord, out canOpen, out canClose);
		}
		else
		{
			CharHelper.CheckOpenCloseDelimiter(rune, result, emphasisDescriptor.EnableWithinWord, out canOpen, out canClose);
		}
		if (canOpen | canClose)
		{
			DelimiterType delimiterType = DelimiterType.Undefined;
			if (canOpen)
			{
				delimiterType |= DelimiterType.Open;
			}
			if (canClose)
			{
				delimiterType |= DelimiterType.Close;
			}
			EmphasisDelimiterInline inline = new EmphasisDelimiterInline(this, emphasisDescriptor, new StringSlice(slice.Text, start, slice.Start - 1))
			{
				DelimiterCount = num,
				Type = delimiterType,
				Span = new SourceSpan(processor.GetSourcePosition(start, out var lineIndex, out var column), processor.GetSourcePosition(slice.Start - 1)),
				Column = column,
				Line = lineIndex
			};
			processor.Inline = inline;
			return true;
		}
		return false;
	}

	private void ProcessEmphasis(InlineProcessor processor, List<EmphasisDelimiterInline> delimiters)
	{
		for (int i = 0; i < delimiters.Count; i++)
		{
			EmphasisDelimiterInline emphasisDelimiterInline = delimiters[i];
			EmphasisDescriptor emphasisDescriptor = emphasisMap[emphasisDelimiterInline.DelimiterChar];
			if (emphasisDescriptor == null || (emphasisDelimiterInline.Type & DelimiterType.Close) == 0)
			{
				continue;
			}
			while (emphasisDelimiterInline.DelimiterCount >= emphasisDescriptor.MinimumCount)
			{
				EmphasisDelimiterInline emphasisDelimiterInline2 = null;
				int num = -1;
				for (int num2 = i - 1; num2 >= 0; num2--)
				{
					EmphasisDelimiterInline emphasisDelimiterInline3 = delimiters[num2];
					bool flag = ((emphasisDelimiterInline.Type & DelimiterType.Open) != DelimiterType.Undefined || (emphasisDelimiterInline3.Type & DelimiterType.Close) != DelimiterType.Undefined) && emphasisDelimiterInline3.DelimiterCount != emphasisDelimiterInline.DelimiterCount && (emphasisDelimiterInline3.DelimiterCount + emphasisDelimiterInline.DelimiterCount) % 3 == 0 && (emphasisDelimiterInline3.DelimiterCount % 3 != 0 || emphasisDelimiterInline.DelimiterCount % 3 != 0);
					if (emphasisDelimiterInline3.DelimiterChar == emphasisDelimiterInline.DelimiterChar && (emphasisDelimiterInline3.Type & DelimiterType.Open) != DelimiterType.Undefined && emphasisDelimiterInline3.DelimiterCount >= emphasisDescriptor.MinimumCount && !flag)
					{
						emphasisDelimiterInline2 = emphasisDelimiterInline3;
						num = num2;
						break;
					}
				}
				if (emphasisDelimiterInline2 != null)
				{
					EmphasisInline emphasisInline;
					while (true)
					{
						int num3 = Math.Min(Math.Min(emphasisDelimiterInline2.DelimiterCount, emphasisDelimiterInline.DelimiterCount), emphasisDescriptor.MaximumCount);
						emphasisInline = null;
						for (int num4 = TryCreateEmphasisInlineList.Count - 1; num4 >= 0; num4--)
						{
							emphasisInline = TryCreateEmphasisInlineList[num4](emphasisDelimiterInline.DelimiterChar, num3);
							if (emphasisInline != null)
							{
								break;
							}
						}
						if (emphasisInline == null)
						{
							emphasisInline = new EmphasisInline
							{
								DelimiterChar = emphasisDelimiterInline.DelimiterChar,
								DelimiterCount = num3
							};
						}
						int delimiterCount = emphasisDelimiterInline2.DelimiterCount;
						int delimiterCount2 = emphasisDelimiterInline.DelimiterCount;
						emphasisInline.Span.Start = emphasisDelimiterInline2.Span.Start + delimiterCount - num3;
						emphasisInline.Line = emphasisDelimiterInline2.Line;
						emphasisInline.Column = emphasisDelimiterInline2.Column + delimiterCount - num3;
						emphasisInline.Span.End = emphasisDelimiterInline.Span.End - delimiterCount2 + num3;
						emphasisDelimiterInline2.Span.End -= num3;
						emphasisDelimiterInline2.Content.End -= num3;
						emphasisDelimiterInline.Content.Start += num3;
						emphasisDelimiterInline.Span.Start += num3;
						emphasisDelimiterInline.Column += num3;
						emphasisDelimiterInline2.DelimiterCount -= num3;
						emphasisDelimiterInline.DelimiterCount -= num3;
						EmphasisDelimiterInline emphasisDelimiterInline4 = emphasisDelimiterInline2;
						HtmlAttributes htmlAttributes = emphasisDelimiterInline.TryGetAttributes();
						if (htmlAttributes != null)
						{
							emphasisInline.SetAttributes(htmlAttributes);
						}
						emphasisDelimiterInline4.EmbraceChildrenBy(emphasisInline);
						for (int num5 = i - 1; num5 >= num + 1; num5--)
						{
							EmphasisDelimiterInline emphasisDelimiterInline5 = delimiters[num5];
							emphasisDelimiterInline5.ReplaceBy(emphasisDelimiterInline5.AsLiteralInline());
							delimiters.RemoveAt(num5);
							i--;
						}
						if (emphasisDelimiterInline.DelimiterCount == 0)
						{
							break;
						}
						if (emphasisDelimiterInline2.DelimiterCount >= emphasisDescriptor.MinimumCount && emphasisDelimiterInline.DelimiterCount >= emphasisDescriptor.MinimumCount)
						{
							continue;
						}
						goto IL_0325;
					}
					ContainerInline parent = ((emphasisDelimiterInline2.DelimiterCount > 0) ? emphasisInline : emphasisInline.Parent);
					emphasisDelimiterInline.MoveChildrenAfter(parent);
					emphasisDelimiterInline.Remove();
					delimiters.RemoveAt(i);
					i--;
					if (emphasisDelimiterInline2.DelimiterCount == 0)
					{
						emphasisDelimiterInline2.MoveChildrenAfter(emphasisDelimiterInline2);
						emphasisDelimiterInline2.Remove();
						delimiters.RemoveAt(num);
						i--;
					}
				}
				else if ((emphasisDelimiterInline.Type & DelimiterType.Open) == 0)
				{
					emphasisDelimiterInline.ReplaceBy(emphasisDelimiterInline.AsLiteralInline());
					delimiters.RemoveAt(i);
					i--;
				}
				break;
				IL_0325:
				if (emphasisDelimiterInline2.DelimiterCount > 0)
				{
					emphasisDelimiterInline2.ReplaceBy(emphasisDelimiterInline2.AsLiteralInline());
					delimiters.RemoveAt(num);
					i--;
					continue;
				}
				Inline firstChild = emphasisDelimiterInline2.FirstChild;
				firstChild.Remove();
				emphasisDelimiterInline2.ReplaceBy(firstChild);
				firstChild.IsClosed = true;
				emphasisDelimiterInline.Remove();
				firstChild.InsertAfter(emphasisDelimiterInline);
				delimiters.RemoveAt(num);
				i--;
			}
		}
		for (int j = 0; j < delimiters.Count; j++)
		{
			EmphasisDelimiterInline emphasisDelimiterInline6 = delimiters[j];
			emphasisDelimiterInline6.ReplaceBy(emphasisDelimiterInline6.AsLiteralInline());
		}
		delimiters.Clear();
	}
}
