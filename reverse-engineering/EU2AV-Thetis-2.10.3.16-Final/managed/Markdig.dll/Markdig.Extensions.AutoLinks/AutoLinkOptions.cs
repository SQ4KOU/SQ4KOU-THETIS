using Markdig.Parsers;

namespace Markdig.Extensions.AutoLinks;

public class AutoLinkOptions : LinkOptions
{
	public string ValidPreviousCharacters { get; set; }

	public bool UseHttpsForWWWLinks { get; set; }

	public bool AllowDomainWithoutPeriod { get; set; }

	public AutoLinkOptions()
	{
		ValidPreviousCharacters = "*_~(";
	}
}
