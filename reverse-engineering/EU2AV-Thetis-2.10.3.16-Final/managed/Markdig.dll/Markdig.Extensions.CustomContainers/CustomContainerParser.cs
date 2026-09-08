using Markdig.Parsers;

namespace Markdig.Extensions.CustomContainers;

public class CustomContainerParser : FencedBlockParserBase<CustomContainer>
{
	public CustomContainerParser()
	{
		base.OpeningCharacters = new char[1] { ':' };
		base.InfoPrefix = null;
	}

	protected override CustomContainer CreateFencedBlock(BlockProcessor processor)
	{
		return new CustomContainer(this);
	}
}
