namespace Markdig.Parsers;

public abstract class ListItemParser
{
	public char[]? OpeningCharacters { get; protected set; }

	public abstract bool TryParse(BlockProcessor state, char pendingBulletType, out ListInfo result);
}
