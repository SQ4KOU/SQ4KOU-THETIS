using Markdig.Syntax.Inlines;

namespace Markdig.Parsers;

public delegate void ProcessInlineDelegate(InlineProcessor processor, Inline? inline);
