using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Extensions.Alerts;

public class AlertBlock : QuoteBlock
{
	public StringSlice Kind { get; set; }

	public StringSlice TriviaSpaceAfterKind { get; set; }

	public AlertBlock(StringSlice kind)
		: base(null)
	{
		Kind = kind;
	}
}
