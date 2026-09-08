using Markdig.Renderers;
using Markdig.Renderers.Roundtrip;

namespace Markdig.Extensions.Yaml;

public class YamlFrontMatterRoundtripRenderer : MarkdownObjectRenderer<RoundtripRenderer, YamlFrontMatterBlock>
{
	private readonly CodeBlockRenderer _codeBlockRenderer;

	public YamlFrontMatterRoundtripRenderer()
	{
		_codeBlockRenderer = new CodeBlockRenderer();
	}

	protected override void Write(RoundtripRenderer renderer, YamlFrontMatterBlock obj)
	{
		renderer.Writer.WriteLine("---");
		_codeBlockRenderer.Write(renderer, obj);
		renderer.Writer.WriteLine("---");
	}
}
