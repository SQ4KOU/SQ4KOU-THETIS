using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Html.Inlines;

public class EmphasisInlineRenderer : HtmlObjectRenderer<EmphasisInline>
{
	public delegate string? GetTagDelegate(EmphasisInline obj);

	public GetTagDelegate GetTag { get; set; }

	public EmphasisInlineRenderer()
	{
		GetTag = GetDefaultTag;
	}

	protected override void Write(HtmlRenderer renderer, EmphasisInline obj)
	{
		string content = null;
		if (renderer.EnableHtmlForInline)
		{
			content = GetTag(obj);
			renderer.Write('<');
			renderer.WriteRaw(content);
			renderer.WriteAttributes(obj);
			renderer.WriteRaw('>');
		}
		renderer.WriteChildren(obj);
		if (renderer.EnableHtmlForInline)
		{
			renderer.Write("</");
			renderer.WriteRaw(content);
			renderer.WriteRaw('>');
		}
	}

	public static string? GetDefaultTag(EmphasisInline obj)
	{
		char delimiterChar = obj.DelimiterChar;
		if ((delimiterChar == '*' || delimiterChar == '_') ? true : false)
		{
			if (obj.DelimiterCount != 2)
			{
				return "em";
			}
			return "strong";
		}
		return null;
	}
}
