namespace Markdig.Parsers;

public class UnorderedListItemParser : ListItemParser
{
	public UnorderedListItemParser()
	{
		base.OpeningCharacters = new char[3] { '-', '+', '*' };
	}

	public override bool TryParse(BlockProcessor state, char pendingBulletType, out ListInfo result)
	{
		result = new ListInfo(state.CurrentChar);
		state.NextChar();
		return true;
	}
}
