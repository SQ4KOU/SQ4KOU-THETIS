namespace ExCSS;

public interface IDocumentFunction : IStylesheetNode, IStyleFormattable
{
	string Name { get; }

	string Data { get; }
}
