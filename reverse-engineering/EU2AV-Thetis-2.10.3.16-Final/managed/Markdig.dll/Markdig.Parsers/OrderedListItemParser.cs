namespace Markdig.Parsers;

public abstract class OrderedListItemParser : ListItemParser
{
	public char[] OrderedDelimiters { get; set; }

	protected OrderedListItemParser()
	{
		OrderedDelimiters = new char[2] { '.', ')' };
	}

	protected bool TryParseDelimiter(BlockProcessor state, out char orderedDelimiter)
	{
		orderedDelimiter = state.CurrentChar;
		char[] orderedDelimiters = OrderedDelimiters;
		for (int i = 0; i < orderedDelimiters.Length; i++)
		{
			if (orderedDelimiters[i] == orderedDelimiter)
			{
				state.NextChar();
				return true;
			}
		}
		return false;
	}
}
