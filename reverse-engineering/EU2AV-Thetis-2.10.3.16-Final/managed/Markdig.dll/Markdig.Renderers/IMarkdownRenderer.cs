using System;
using Markdig.Syntax;

namespace Markdig.Renderers;

public interface IMarkdownRenderer
{
	ObjectRendererCollection ObjectRenderers { get; }

	event Action<IMarkdownRenderer, MarkdownObject> ObjectWriteBefore;

	event Action<IMarkdownRenderer, MarkdownObject> ObjectWriteAfter;

	object Render(MarkdownObject markdownObject);
}
