using Markdig.Syntax;

namespace Markdig.Parsers;

public abstract class BlockParser : ParserBase<BlockProcessor>, IBlockParser<BlockProcessor>, IMarkdownParser<BlockProcessor>
{
	internal ProcessBlockDelegate? GetClosedEvent => Closed;

	public event ProcessBlockDelegate? Closed;

	public bool HasOpeningCharacter(char c)
	{
		if (base.OpeningCharacters != null)
		{
			char[] openingCharacters = base.OpeningCharacters;
			for (int i = 0; i < openingCharacters.Length; i++)
			{
				if (openingCharacters[i] == c)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool CanInterrupt(BlockProcessor processor, Block block)
	{
		return true;
	}

	public abstract BlockState TryOpen(BlockProcessor processor);

	public virtual BlockState TryContinue(BlockProcessor processor, Block block)
	{
		return BlockState.None;
	}

	public virtual bool Close(BlockProcessor processor, Block block)
	{
		return true;
	}
}
