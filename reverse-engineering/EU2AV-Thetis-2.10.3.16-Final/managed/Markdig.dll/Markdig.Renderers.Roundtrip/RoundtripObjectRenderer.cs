using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public abstract class RoundtripObjectRenderer<TObject> : MarkdownObjectRenderer<RoundtripRenderer, TObject> where TObject : MarkdownObject
{
}
