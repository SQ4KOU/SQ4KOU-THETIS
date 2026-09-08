using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class LinkReferenceDefinitionRenderer : NormalizeObjectRenderer<LinkReferenceDefinition>
{
	protected override void Write(NormalizeRenderer renderer, LinkReferenceDefinition linkDef)
	{
		renderer.EnsureLine();
		renderer.Write('[');
		renderer.Write(linkDef.Label);
		renderer.Write("]: ");
		renderer.Write(linkDef.Url);
		if (linkDef.Title != null)
		{
			renderer.Write(" \"");
			renderer.Write(linkDef.Title.Replace("\"", "\\\""));
			renderer.Write('"');
		}
		renderer.FinishBlock(emptyLine: false);
	}
}
