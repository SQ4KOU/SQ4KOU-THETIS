using System;
using Markdig.Syntax;

namespace Markdig.Renderers;

public interface IMarkdownObjectRenderer
{
	bool Accept(RendererBase renderer, Type objectType);

	void Write(RendererBase renderer, MarkdownObject objectToRender);
}
