namespace Markdig.Parsers;

public interface IMarkdownParser<in TProcessor>
{
	char[]? OpeningCharacters { get; }

	int Index { get; }

	void Initialize();
}
