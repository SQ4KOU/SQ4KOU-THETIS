using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Footnotes;

public class FootnoteLink : Inline
{
	public bool IsBackLink { get; set; }

	public int Index { get; set; }

	public Footnote Footnote { get; set; }

	public FootnoteLink(Footnote footnote)
	{
		Footnote = footnote;
	}
}
