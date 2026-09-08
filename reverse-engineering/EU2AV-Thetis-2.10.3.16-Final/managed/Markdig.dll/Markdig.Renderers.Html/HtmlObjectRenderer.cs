using Markdig.Syntax;

namespace Markdig.Renderers.Html;

public abstract class HtmlObjectRenderer<TObject> : MarkdownObjectRenderer<HtmlRenderer, TObject> where TObject : MarkdownObject
{
}
