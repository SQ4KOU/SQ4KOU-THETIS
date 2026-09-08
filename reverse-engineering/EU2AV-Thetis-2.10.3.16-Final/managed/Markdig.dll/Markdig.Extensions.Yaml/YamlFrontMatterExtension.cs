using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Roundtrip;

namespace Markdig.Extensions.Yaml;

public class YamlFrontMatterExtension : IMarkdownExtension
{
	public bool AllowInMiddleOfDocument { get; set; }

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.BlockParsers.Contains<YamlFrontMatterParser>())
		{
			pipeline.BlockParsers.InsertBefore<ThematicBreakParser>(new YamlFrontMatterParser
			{
				AllowInMiddleOfDocument = AllowInMiddleOfDocument
			});
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (!renderer.ObjectRenderers.Contains<YamlFrontMatterHtmlRenderer>())
		{
			renderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Html.CodeBlockRenderer>(new YamlFrontMatterHtmlRenderer());
		}
		if (!renderer.ObjectRenderers.Contains<YamlFrontMatterRoundtripRenderer>())
		{
			renderer.ObjectRenderers.InsertBefore<Markdig.Renderers.Roundtrip.CodeBlockRenderer>(new YamlFrontMatterRoundtripRenderer());
		}
	}
}
