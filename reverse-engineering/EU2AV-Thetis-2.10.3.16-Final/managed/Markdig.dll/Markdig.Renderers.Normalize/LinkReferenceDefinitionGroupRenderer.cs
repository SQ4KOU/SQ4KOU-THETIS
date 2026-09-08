using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class LinkReferenceDefinitionGroupRenderer : NormalizeObjectRenderer<LinkReferenceDefinitionGroup>
{
	protected override void Write(NormalizeRenderer renderer, LinkReferenceDefinitionGroup obj)
	{
		renderer.EnsureLine();
		renderer.WriteChildren(obj);
		renderer.FinishBlock(emptyLine: false);
	}
}
