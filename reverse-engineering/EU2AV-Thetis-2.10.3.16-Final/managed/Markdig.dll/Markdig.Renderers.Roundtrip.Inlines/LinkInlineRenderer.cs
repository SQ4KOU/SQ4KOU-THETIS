using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Roundtrip.Inlines;

public class LinkInlineRenderer : RoundtripObjectRenderer<LinkInline>
{
	protected override void Write(RoundtripRenderer renderer, LinkInline link)
	{
		if (link.IsImage)
		{
			renderer.Write('!');
		}
		renderer.Write('[');
		renderer.WriteChildren(link);
		renderer.Write(']');
		if (link.Label != null)
		{
			if (link.LocalLabel == LocalLabel.Local || link.LocalLabel == LocalLabel.Empty)
			{
				renderer.Write('[');
				if (link.LocalLabel == LocalLabel.Local)
				{
					renderer.Write(link.LabelWithTrivia);
				}
				renderer.Write(']');
			}
		}
		else
		{
			if (link.Url == null)
			{
				return;
			}
			renderer.Write('(');
			renderer.Write(link.TriviaBeforeUrl);
			if (link.UrlHasPointyBrackets)
			{
				renderer.Write('<');
			}
			renderer.Write(link.UnescapedUrl);
			if (link.UrlHasPointyBrackets)
			{
				renderer.Write('>');
			}
			renderer.Write(link.TriviaAfterUrl);
			if (!string.IsNullOrEmpty(link.Title))
			{
				char titleEnclosingCharacter = link.TitleEnclosingCharacter;
				char content = link.TitleEnclosingCharacter;
				if (link.TitleEnclosingCharacter == '(')
				{
					content = ')';
				}
				renderer.Write(titleEnclosingCharacter);
				renderer.Write(link.UnescapedTitle);
				renderer.Write(content);
				renderer.Write(link.TriviaAfterTitle);
			}
			renderer.Write(')');
		}
	}
}
