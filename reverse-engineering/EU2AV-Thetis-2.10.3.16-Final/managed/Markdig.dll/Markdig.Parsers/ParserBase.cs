namespace Markdig.Parsers;

public abstract class ParserBase<TProcessor> : IMarkdownParser<TProcessor>
{
	public char[]? OpeningCharacters { get; set; }

	public int Index { get; internal set; }

	public virtual void Initialize()
	{
	}
}
