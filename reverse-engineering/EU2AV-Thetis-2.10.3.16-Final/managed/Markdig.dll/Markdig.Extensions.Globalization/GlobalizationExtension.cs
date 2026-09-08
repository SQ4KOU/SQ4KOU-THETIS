using System.Collections.Generic;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Globalization;

public class GlobalizationExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.DocumentProcessed -= Pipeline_DocumentProcessed;
		pipeline.DocumentProcessed += Pipeline_DocumentProcessed;
	}

	private void Pipeline_DocumentProcessed(MarkdownDocument document)
	{
		foreach (MarkdownObject item in document.Descendants())
		{
			if (!(item is TableRow) && !(item is TableCell) && !(item is ListItemBlock) && ShouldBeRightToLeft(item))
			{
				HtmlAttributes attributes = item.GetAttributes();
				attributes.AddPropertyIfNotExist("dir", "rtl");
				if (item is Table)
				{
					attributes.AddPropertyIfNotExist("align", "right");
				}
			}
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}

	private static bool ShouldBeRightToLeft(MarkdownObject item)
	{
		if (item is IEnumerable<MarkdownObject> enumerable)
		{
			foreach (MarkdownObject item2 in enumerable)
			{
				if (!(item2 is TaskList))
				{
					return ShouldBeRightToLeft(item2);
				}
			}
		}
		else
		{
			if (item is LeafBlock leafBlock)
			{
				return ShouldBeRightToLeft(leafBlock.Inline);
			}
			if (item is LiteralInline literalInline)
			{
				return StartsWithRtlCharacter(literalInline.Content);
			}
		}
		foreach (ParagraphBlock item3 in item.Descendants<ParagraphBlock>())
		{
			foreach (Inline item4 in item3.Inline)
			{
				if (item4 is LiteralInline literalInline2)
				{
					return StartsWithRtlCharacter(literalInline2.Content);
				}
			}
		}
		return false;
	}

	private static bool StartsWithRtlCharacter(StringSlice slice)
	{
		for (int i = slice.Start; i <= slice.End; i++)
		{
			char c = slice[i];
			if (c < '\u0080')
			{
				if (c.IsAlpha())
				{
					return false;
				}
				continue;
			}
			int c2 = c;
			if (char.IsHighSurrogate(c) && i < slice.End && char.IsLowSurrogate(slice[i + 1]))
			{
				c2 = char.ConvertToUtf32(c, slice[i + 1]);
				i++;
			}
			if (CharHelper.IsRightToLeft(c2))
			{
				return true;
			}
			if (CharHelper.IsLeftToRight(c2))
			{
				return false;
			}
		}
		return false;
	}
}
