using System.IO;
using Markdig.Helpers;
using Markdig.Renderers.Roundtrip.Inlines;
using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class RoundtripRenderer : TextRendererBase<RoundtripRenderer>
{
	public RoundtripRenderer(TextWriter writer)
		: base(writer)
	{
		base.ObjectRenderers.Add(new CodeBlockRenderer());
		base.ObjectRenderers.Add(new ListRenderer());
		base.ObjectRenderers.Add(new HeadingRenderer());
		base.ObjectRenderers.Add(new HtmlBlockRenderer());
		base.ObjectRenderers.Add(new ParagraphRenderer());
		base.ObjectRenderers.Add(new QuoteBlockRenderer());
		base.ObjectRenderers.Add(new ThematicBreakRenderer());
		base.ObjectRenderers.Add(new LinkReferenceDefinitionGroupRenderer());
		base.ObjectRenderers.Add(new LinkReferenceDefinitionRenderer());
		base.ObjectRenderers.Add(new EmptyBlockRenderer());
		base.ObjectRenderers.Add(new AutolinkInlineRenderer());
		base.ObjectRenderers.Add(new CodeInlineRenderer());
		base.ObjectRenderers.Add(new DelimiterInlineRenderer());
		base.ObjectRenderers.Add(new EmphasisInlineRenderer());
		base.ObjectRenderers.Add(new LineBreakInlineRenderer());
		base.ObjectRenderers.Add(new RoundtripHtmlInlineRenderer());
		base.ObjectRenderers.Add(new RoundtripHtmlEntityInlineRenderer());
		base.ObjectRenderers.Add(new LinkInlineRenderer());
		base.ObjectRenderers.Add(new LiteralInlineRenderer());
	}

	public void WriteLeafRawLines(LeafBlock leafBlock)
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
				StringSlice slice = lines2[i].Slice;
				Write(ref slice);
				WriteLine(slice.NewLine);
			}
		}
	}

	public void RenderLinesBefore(Block block)
	{
		if (block.LinesBefore == null)
		{
			return;
		}
		foreach (StringSlice item in block.LinesBefore)
		{
			Write(item);
			WriteLine(item.NewLine);
		}
	}

	public void RenderLinesAfter(Block block)
	{
		previousWasLine = true;
		if (block.LinesAfter == null)
		{
			return;
		}
		foreach (StringSlice item in block.LinesAfter)
		{
			Write(item);
			WriteLine(item.NewLine);
		}
	}
}
