using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public abstract class NormalizeObjectRenderer<TObject> : MarkdownObjectRenderer<NormalizeRenderer, TObject> where TObject : MarkdownObject
{
}
