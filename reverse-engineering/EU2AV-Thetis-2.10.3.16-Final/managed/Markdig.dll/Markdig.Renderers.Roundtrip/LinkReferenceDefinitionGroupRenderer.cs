using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class LinkReferenceDefinitionGroupRenderer : RoundtripObjectRenderer<LinkReferenceDefinitionGroup>
{
	protected override void Write(RoundtripRenderer renderer, LinkReferenceDefinitionGroup obj)
	{
		renderer.WriteChildren(obj);
	}
}
