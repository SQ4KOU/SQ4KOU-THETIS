namespace Markdig.Syntax.Inlines;

public interface IInline : IMarkdownObject
{
	ContainerInline? Parent { get; }

	Inline? PreviousSibling { get; }

	Inline? NextSibling { get; }

	bool IsClosed { get; set; }
}
