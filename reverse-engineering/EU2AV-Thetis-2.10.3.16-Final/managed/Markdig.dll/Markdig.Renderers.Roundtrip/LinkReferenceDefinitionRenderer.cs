using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class LinkReferenceDefinitionRenderer : RoundtripObjectRenderer<LinkReferenceDefinition>
{
	protected override void Write(RoundtripRenderer renderer, LinkReferenceDefinition linkDef)
	{
		renderer.RenderLinesBefore(linkDef);
		renderer.Write(linkDef.TriviaBefore);
		renderer.Write('[');
		renderer.Write(linkDef.LabelWithTrivia);
		renderer.Write("]:");
		renderer.Write(linkDef.TriviaBeforeUrl);
		if (linkDef.UrlHasPointyBrackets)
		{
			renderer.Write('<');
		}
		renderer.Write(linkDef.UnescapedUrl);
		if (linkDef.UrlHasPointyBrackets)
		{
			renderer.Write('>');
		}
		renderer.Write(linkDef.TriviaBeforeTitle);
		if (linkDef.Title != null)
		{
			char titleEnclosingCharacter = linkDef.TitleEnclosingCharacter;
			char content = linkDef.TitleEnclosingCharacter;
			if (linkDef.TitleEnclosingCharacter == '(')
			{
				content = ')';
			}
			renderer.Write(titleEnclosingCharacter);
			renderer.Write(linkDef.UnescapedTitle);
			renderer.Write(content);
		}
		renderer.Write(linkDef.TriviaAfter);
		renderer.Write(linkDef.NewLine.AsString());
		renderer.RenderLinesAfter(linkDef);
	}
}
