using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.Yaml;

public class YamlFrontMatterHtmlRenderer : HtmlObjectRenderer<YamlFrontMatterBlock>
{
	protected override void Write(HtmlRenderer renderer, YamlFrontMatterBlock obj)
	{
	}
}
