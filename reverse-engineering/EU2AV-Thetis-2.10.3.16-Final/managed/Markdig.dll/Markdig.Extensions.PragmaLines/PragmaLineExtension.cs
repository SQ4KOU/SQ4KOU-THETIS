using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.PragmaLines;

public class PragmaLineExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
		pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}

	private static void PipelineOnDocumentProcessed(MarkdownDocument document)
	{
		int index = 0;
		AddPragmas(document, ref index);
	}

	private static void AddPragmas(Block block, ref int index)
	{
		HtmlAttributes attributes = block.GetAttributes();
		string pragmaId = GetPragmaId(block);
		if (attributes.Id == null)
		{
			attributes.Id = pragmaId;
		}
		else if (block.Parent != null)
		{
			HeadingBlock headingBlock = block as HeadingBlock;
			string text = "<a id=\"" + pragmaId + "\"></a>";
			if (headingBlock?.Inline?.FirstChild != null)
			{
				headingBlock.Inline.FirstChild.InsertBefore(new HtmlInline(text));
			}
			else
			{
				block.Parent.Insert(index, new HtmlBlock(null)
				{
					Lines = new StringLineGroup(text)
				});
				index++;
			}
		}
		if (block is ContainerBlock containerBlock)
		{
			for (int i = 0; i < containerBlock.Count; i++)
			{
				AddPragmas(containerBlock[i], ref i);
			}
		}
	}

	private static string GetPragmaId(Block block)
	{
		return $"pragma-line-{block.Line}";
	}
}
