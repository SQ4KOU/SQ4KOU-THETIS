using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Markdig.Extensions.SmartyPants;

public class SmartyPantsExtension : IMarkdownExtension
{
	public SmartyPantOptions Options { get; }

	public SmartyPantsExtension(SmartyPantOptions? options)
	{
		Options = options ?? new SmartyPantOptions();
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<SmartyPantsInlineParser>())
		{
			pipeline.InlineParsers.InsertAfter<CodeInlineParser>(new SmartyPantsInlineParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<HtmlSmartyPantRenderer>())
		{
			htmlRenderer.ObjectRenderers.Add(new HtmlSmartyPantRenderer(Options));
		}
	}
}
