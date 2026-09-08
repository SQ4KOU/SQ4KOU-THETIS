namespace Markdig.Syntax;

public sealed class BlankLineBlock : Block
{
	public BlankLineBlock()
		: base(null)
	{
		base.IsOpen = false;
	}
}
