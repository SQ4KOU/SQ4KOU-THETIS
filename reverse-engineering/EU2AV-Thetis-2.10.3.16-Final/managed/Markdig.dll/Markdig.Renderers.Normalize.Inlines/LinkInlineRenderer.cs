using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class LinkInlineRenderer : NormalizeObjectRenderer<LinkInline>
{
	protected override void Write(NormalizeRenderer renderer, LinkInline link)
	{
		if (link.IsAutoLink && !renderer.Options.ExpandAutoLinks)
		{
			renderer.Write(link.Url);
			return;
		}
		if (link.IsImage)
		{
			renderer.Write('!');
		}
		renderer.Write('[');
		renderer.WriteChildren(link);
		renderer.Write(']');
		if (link.Label != null)
		{
			if (link.FirstChild is LiteralInline literalInline && literalInline.Content.Length == link.Label.Length && literalInline.Content.Match(link.Label))
			{
				if (!link.IsShortcut)
				{
					renderer.Write("[]");
				}
			}
			else
			{
				renderer.Write('[').Write(link.Label).Write(']');
			}
		}
		else if (!string.IsNullOrEmpty(link.Url))
		{
			renderer.Write('(').Write(link.Url);
			string title = link.Title;
			if (title != null && title.Length > 0)
			{
				renderer.Write(" \"");
				renderer.Write(link.Title.Replace("\"", "\\\""));
				renderer.Write('"');
			}
			renderer.Write(')');
		}
	}
}
