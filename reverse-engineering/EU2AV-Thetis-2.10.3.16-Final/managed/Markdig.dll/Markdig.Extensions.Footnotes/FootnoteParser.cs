using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Footnotes;

public class FootnoteParser : BlockParser
{
	private static readonly object DocumentKey = typeof(Footnote);

	public FootnoteParser()
	{
		base.OpeningCharacters = new char[1] { '[' };
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		return TryOpen(processor, isContinue: false);
	}

	private BlockState TryOpen(BlockProcessor processor, bool isContinue)
	{
		ContainerBlock currentContainerOpened = processor.GetCurrentContainerOpened();
		if (processor.IsCodeIndent || (!isContinue && currentContainerOpened.GetType() != typeof(MarkdownDocument)) || (isContinue && !(currentContainerOpened is FootnoteGroup)))
		{
			return BlockState.None;
		}
		int column = processor.Column;
		int start = processor.Start;
		if (!LinkHelper.TryParseLabel(ref processor.Line, allowEmpty: false, out string label, out SourceSpan labelSpan) || !label.StartsWith("^") || processor.CurrentChar != ':')
		{
			processor.GoToColumn(column);
			return BlockState.None;
		}
		int num = processor.Start - start;
		processor.Column += num;
		processor.NextChar();
		Footnote footnote = new Footnote(this)
		{
			Label = label,
			LabelSpan = labelSpan,
			Column = processor.Column,
			Span = new SourceSpan(processor.Start, processor.Line.End)
		};
		FootnoteGroup footnoteGroup = processor.Document.GetData(DocumentKey) as FootnoteGroup;
		if (footnoteGroup == null)
		{
			footnoteGroup = new FootnoteGroup(this);
			processor.Document.Add(footnoteGroup);
			processor.Document.SetData(DocumentKey, footnoteGroup);
			processor.Document.ProcessInlinesEnd += Document_ProcessInlinesEnd;
		}
		footnoteGroup.Add(footnote);
		FootnoteLinkReferenceDefinition linkReferenceDefinition = new FootnoteLinkReferenceDefinition(footnote)
		{
			CreateLinkInline = CreateLinkToFootnote,
			Line = processor.LineIndex,
			Column = column,
			Span = new SourceSpan(start, processor.Start - 2),
			LabelSpan = labelSpan,
			Label = label
		};
		processor.Document.SetLinkReferenceDefinition(footnote.Label, linkReferenceDefinition, addGroup: true);
		processor.NewBlocks.Push(footnote);
		return BlockState.Continue;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		Footnote footnote = (Footnote)block;
		if (processor.CurrentBlock == null || processor.CurrentBlock.IsBreakable)
		{
			if (processor.IsBlankLine)
			{
				footnote.IsLastLineEmpty = true;
				return BlockState.ContinueDiscard;
			}
			if (processor.Column == 0)
			{
				if (footnote.IsLastLineEmpty)
				{
					processor.Close(footnote);
					return TryOpen(processor);
				}
				if (TryOpen(processor, isContinue: true) == BlockState.Continue)
				{
					processor.Close(footnote);
					return BlockState.Continue;
				}
			}
		}
		footnote.IsLastLineEmpty = false;
		if (processor.IsCodeIndent)
		{
			processor.GoToCodeIndent();
		}
		return BlockState.Continue;
	}

	private void Document_ProcessInlinesEnd(InlineProcessor state, Inline? inline)
	{
		FootnoteGroup footnoteGroup = (FootnoteGroup)state.Document.GetData(DocumentKey);
		state.Document.Remove(footnoteGroup);
		state.Document.Add(footnoteGroup);
		state.Document.RemoveData(DocumentKey);
		footnoteGroup.Sort(delegate(Block leftObj, Block rightObj)
		{
			Footnote footnote2 = (Footnote)leftObj;
			Footnote footnote3 = (Footnote)rightObj;
			return (footnote2.Order >= 0 && footnote3.Order >= 0) ? footnote2.Order.CompareTo(footnote3.Order) : 0;
		});
		int num = 0;
		for (int num2 = 0; num2 < footnoteGroup.Count; num2++)
		{
			Footnote footnote = (Footnote)footnoteGroup[num2];
			if (footnote.Order < 0)
			{
				footnoteGroup.RemoveAt(num2);
				num2--;
				continue;
			}
			ParagraphBlock paragraphBlock = footnote.LastChild as ParagraphBlock;
			if (paragraphBlock == null)
			{
				paragraphBlock = new ParagraphBlock();
				footnote.Add(paragraphBlock);
			}
			ParagraphBlock paragraphBlock2 = paragraphBlock;
			if (paragraphBlock2.Inline == null)
			{
				ContainerInline containerInline = (paragraphBlock2.Inline = new ContainerInline());
			}
			foreach (FootnoteLink link in footnote.Links)
			{
				num = (link.Index = num + 1);
				FootnoteLink child = new FootnoteLink(footnote)
				{
					Index = num,
					IsBackLink = true
				};
				paragraphBlock.Inline.AppendChild(child);
			}
		}
	}

	private static Inline CreateLinkToFootnote(InlineProcessor state, LinkReferenceDefinition linkRef, Inline? child)
	{
		Footnote footnote = ((FootnoteLinkReferenceDefinition)linkRef).Footnote;
		if (footnote.Order < 0)
		{
			FootnoteGroup footnoteGroup = (FootnoteGroup)state.Document.GetData(DocumentKey);
			footnoteGroup.CurrentOrder++;
			footnote.Order = footnoteGroup.CurrentOrder;
		}
		FootnoteLink footnoteLink = new FootnoteLink(footnote);
		footnote.Links.Add(footnoteLink);
		return footnoteLink;
	}
}
