using Markdig.Syntax;

namespace Markdig.Extensions.Footnotes;

public class FootnoteLinkReferenceDefinition : LinkReferenceDefinition
{
	public Footnote Footnote { get; set; }

	public FootnoteLinkReferenceDefinition(Footnote footnote)
	{
		Footnote = footnote;
	}
}
