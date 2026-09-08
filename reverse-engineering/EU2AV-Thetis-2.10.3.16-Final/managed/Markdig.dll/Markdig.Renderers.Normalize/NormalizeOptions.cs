namespace Markdig.Renderers.Normalize;

public class NormalizeOptions
{
	public bool SpaceAfterQuoteBlock { get; set; }

	public bool EmptyLineAfterCodeBlock { get; set; }

	public bool EmptyLineAfterHeading { get; set; }

	public bool EmptyLineAfterThematicBreak { get; set; }

	public char? ListItemCharacter { get; set; }

	public bool ExpandAutoLinks { get; set; }

	public NormalizeOptions()
	{
		SpaceAfterQuoteBlock = true;
		EmptyLineAfterCodeBlock = true;
		EmptyLineAfterHeading = true;
		EmptyLineAfterThematicBreak = true;
		ExpandAutoLinks = true;
		ListItemCharacter = null;
	}
}
