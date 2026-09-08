using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public static class HtmlAttributesExtensions
{
	private static readonly object Key = typeof(HtmlAttributes);

	public static HtmlAttributes? TryGetAttributes(this IMarkdownObject obj)
	{
		return obj.GetData(Key) as HtmlAttributes;
	}

	public static HtmlAttributes GetAttributes(this IMarkdownObject obj)
	{
		HtmlAttributes htmlAttributes = obj.GetData(Key) as HtmlAttributes;
		if (htmlAttributes == null)
		{
			htmlAttributes = new HtmlAttributes();
			obj.SetAttributes(htmlAttributes);
		}
		return htmlAttributes;
	}

	public static void SetAttributes(this IMarkdownObject obj, HtmlAttributes attributes)
	{
		obj.SetData(Key, attributes);
	}
}
