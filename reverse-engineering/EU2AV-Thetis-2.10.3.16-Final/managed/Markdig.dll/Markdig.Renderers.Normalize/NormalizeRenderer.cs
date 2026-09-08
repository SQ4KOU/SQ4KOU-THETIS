using System.IO;
using Markdig.Helpers;
using Markdig.Renderers.Normalize.Inlines;
using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class NormalizeRenderer : TextRendererBase<NormalizeRenderer>
{
	public NormalizeOptions Options { get; }

	public bool CompactParagraph { get; set; }

	public NormalizeRenderer(TextWriter writer, NormalizeOptions? options = null)
		: base(writer)
	{
		Options = options ?? new NormalizeOptions();
		base.ObjectRenderers.Add(new CodeBlockRenderer());
		base.ObjectRenderers.Add(new ListRenderer());
		base.ObjectRenderers.Add(new HeadingRenderer());
		base.ObjectRenderers.Add(new HtmlBlockRenderer());
		base.ObjectRenderers.Add(new ParagraphRenderer());
		base.ObjectRenderers.Add(new QuoteBlockRenderer());
		base.ObjectRenderers.Add(new ThematicBreakRenderer());
		base.ObjectRenderers.Add(new LinkReferenceDefinitionGroupRenderer());
		base.ObjectRenderers.Add(new LinkReferenceDefinitionRenderer());
		base.ObjectRenderers.Add(new AutolinkInlineRenderer());
		base.ObjectRenderers.Add(new CodeInlineRenderer());
		base.ObjectRenderers.Add(new DelimiterInlineRenderer());
		base.ObjectRenderers.Add(new EmphasisInlineRenderer());
		base.ObjectRenderers.Add(new LineBreakInlineRenderer());
		base.ObjectRenderers.Add(new NormalizeHtmlInlineRenderer());
		base.ObjectRenderers.Add(new NormalizeHtmlEntityInlineRenderer());
		base.ObjectRenderers.Add(new LinkInlineRenderer());
		base.ObjectRenderers.Add(new LiteralInlineRenderer());
	}

	public void FinishBlock(bool emptyLine)
	{
		if (!base.IsLastInContainer)
		{
			WriteLine();
			if (emptyLine)
			{
				WriteLine();
			}
		}
	}

	public NormalizeRenderer WriteLeafRawLines(LeafBlock leafBlock, bool writeEndOfLines, bool indent = false)
	{
		if (leafBlock == null)
		{
			ThrowHelper.ArgumentNullException_leafBlock();
		}
		if (leafBlock.Lines.Lines != null)
		{
			StringLineGroup lines = leafBlock.Lines;
			StringLine[] lines2 = lines.Lines;
			for (int i = 0; i < lines.Count; i++)
			{
				if (!writeEndOfLines && i > 0)
				{
					WriteLine();
				}
				if (indent)
				{
					Write("    ");
				}
				Write(ref lines2[i].Slice);
				if (writeEndOfLines)
				{
					WriteLine();
				}
			}
		}
		return this;
	}
}
