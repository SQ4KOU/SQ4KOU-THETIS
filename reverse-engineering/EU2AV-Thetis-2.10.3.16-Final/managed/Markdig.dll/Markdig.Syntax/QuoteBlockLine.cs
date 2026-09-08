using Markdig.Helpers;

namespace Markdig.Syntax;

public class QuoteBlockLine
{
	public StringSlice TriviaBefore { get; set; }

	public bool QuoteChar { get; set; }

	public bool HasSpaceAfterQuoteChar { get; set; }

	public StringSlice TriviaAfter { get; set; }

	public NewLine NewLine { get; set; }
}
