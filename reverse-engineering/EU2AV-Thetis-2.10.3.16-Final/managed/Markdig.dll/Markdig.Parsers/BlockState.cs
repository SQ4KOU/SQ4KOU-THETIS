namespace Markdig.Parsers;

public enum BlockState
{
	None,
	Skip,
	Continue,
	ContinueDiscard,
	Break,
	BreakDiscard
}
