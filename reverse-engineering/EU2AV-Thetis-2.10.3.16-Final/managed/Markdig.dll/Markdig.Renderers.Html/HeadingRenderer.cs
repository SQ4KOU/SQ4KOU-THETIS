using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public class HeadingRenderer : HtmlObjectRenderer<HeadingBlock>
{
	private static readonly string[] HeadingTexts = new string[6] { "h1", "h2", "h3", "h4", "h5", "h6" };

	protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
	{
		int num = obj.Level - 1;
		string[] headingTexts = HeadingTexts;
		string content = (((uint)num < (uint)headingTexts.Length) ? headingTexts[num] : $"h{obj.Level}");
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write('<');
			renderer.WriteRaw(content);
			renderer.WriteAttributes(obj);
			renderer.WriteRaw('>');
		}
		renderer.WriteLeafInline(obj);
		if (renderer.EnableHtmlForBlock)
		{
			renderer.Write("</");
			renderer.WriteRaw(content);
			renderer.WriteLine('>');
		}
		renderer.EnsureLine();
	}
}
